using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.FSharp.Collections;
using Suave.Logging;
using Suave.Sockets;

namespace Suave.AspNetCore
{
    public static class HttpContextExtensions
    {
        public static async Task<Http.HttpContext> ToSuaveHttpContext(
            this HttpContext context,
            bool preserveHttpHeaderCasing)
        {
            var request = context.Request;

            // Get HTTP headers
            var headers = 
                ListModule.OfSeq(
                    request.Headers
                        .Select(h => new Tuple<string, string>(
                            preserveHttpHeaderCasing ? h.Key : h.Key.ToLower(), 
                            h.Value))
                        .ToList());

            // Get the absolute URL
            var host = request.Host.Value;
            var absoluteUrl = $"{request.Scheme}://{host}{request.Path}{request.QueryString.Value}";

            // Get the raw query string (Suave doesn't include the ? in the beginning)
            var rawQuery = request.QueryString.Value.Substring(Math.Min(request.QueryString.Value.Length, 1));

            // Get files and multipart fields from a form request
            var files = new List<Http.HttpUpload>();
            var multipartFields = new List<Tuple<string, string>>();
            
            if (request.HasFormContentType && request.Form != null && request.Form.Count > 0)
            {
                multipartFields.AddRange(
                    request.Form.Select(field => new Tuple<string, string>(field.Key, field.Value)));

                if (request.Form.Files != null && request.Form.Files.Count > 0)
                {
                    foreach (var file in request.Form.Files)
                    {
                        var tempFileName = Path.GetTempFileName();

                        using (var fileStream = File.Open(tempFileName, FileMode.OpenOrCreate))
                            await file.OpenReadStream().CopyToAsync(fileStream);

                        files.Add(
                            new Http.HttpUpload(
                                file.Name,
                                file.FileName,
                                file.ContentType,
                                tempFileName));
                    }
                }
            }

            // Get the raw body
            // (This will be an empty byte array if the stream has already been
            // read during the form upload)
            var rawForm = await request.Body.ReadAllBytesAsync();
            
            var suaveRequest = new Http.HttpRequest(
                request.Protocol,
                new Uri(absoluteUrl),
                request.Host.Host,
                HttpMethodFromString(request.Method),
                headers,
                rawForm,
                rawQuery,
                ListModule.OfSeq(files),
                ListModule.OfSeq(multipartFields),
                TraceHeader.parseTraceHeaders(headers));

            var suaveRuntime = Http.HttpRuntimeModule.empty;
            var suaveSocketConnection = ConnectionModule.empty;
            return
                Http.HttpContextModule.create(
                    suaveRequest,
                    suaveRuntime,
                    suaveSocketConnection,
                    false);
        }

        public static Http.HttpMethod HttpMethodFromString(string method)
        {
            switch (method)
            {
                case "GET": return Http.HttpMethod.GET;
                case "POST": return Http.HttpMethod.POST;
                case "PUT": return Http.HttpMethod.PUT;
                case "DELETE": return Http.HttpMethod.DELETE;
                case "PATCH": return Http.HttpMethod.PATCH;
                case "CONNECT": return Http.HttpMethod.CONNECT;
                case "HEAD": return Http.HttpMethod.HEAD;
                case "OPTIONS": return Http.HttpMethod.OPTIONS;
                case "TRACE": return Http.HttpMethod.TRACE;
                default: throw new InvalidOperationException($"{method} is not a valid HTTP verb.");
            }
        }

        public static async Task SetResponseFromSuaveResult(this HttpContext context, Http.HttpResult suaveResult)
        {
            // Set HTTP status code
            context.Response.StatusCode = suaveResult.status.code;

            // Set HTTP response headers
            foreach (var header in suaveResult.headers)
            {
                var key = header.Item1;
                var value = header.Item2;

                StringValues existingStringValues;
        
                if(!context.Response.Headers.TryGetValue(key, out existingStringValues))
                {
                    context.Response.Headers[key] = new StringValues(value);
                }
                else 
                {
                    context.Response.Headers[key] = StringValues.Concat(existingStringValues, new StringValues(value));
                }
            }

            // Set HTTP body
            if (suaveResult.content.IsBytes)
            {
                var bytes = ((Http.HttpContent.Bytes)suaveResult.content).Item;
                context.Response.Headers["Content-Length"] = new StringValues(bytes.Length.ToString());
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            }
        }
    }
}