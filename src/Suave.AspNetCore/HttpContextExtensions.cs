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
        public static async Task<Http.HttpContext> ToSuaveHttpContext(this HttpContext context)
        {
            var req = context.Request;

            // Get HTTP headers
            var headers = 
                ListModule.OfSeq(
                    req.Headers
                        .Select(h => new Tuple<string, string>(h.Key, h.Value))
                        .ToList());

            // Get the absolute URL
            var host = context.Request.Host.Value;
            var absoluteUrl = $"{req.Scheme}://{host}{req.Path}{req.QueryString.Value}";

            // Get the raw query string (Suave doesn't include the ? in the beginning)
            var rawQuery = req.QueryString.Value.Substring(Math.Min(req.QueryString.Value.Length, 1));

            // Get files and multipart fields from a form request
            var files = new List<Http.HttpUpload>();
            var multipartFields = new List<Tuple<string, string>>();
            
            if (req.HasFormContentType && req.Form != null && req.Form.Count > 0)
            {
                multipartFields.AddRange(
                    req.Form.Select(field => new Tuple<string, string>(field.Key, field.Value)));

                if (req.Form.Files != null && req.Form.Files.Count > 0)
                {
                    foreach (var file in req.Form.Files)
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
            var rawForm = await req.Body.ReadBytesAsync();

            return Http.HttpContextModule.create(
                new Http.HttpRequest(
                    req.Protocol,
                    new Uri(absoluteUrl),
                    context.Request.Host.Host,
                    HttpMethodFromString(req.Method),
                    headers,
                    rawForm,
                    rawQuery,
                    ListModule.OfSeq(files),
                    ListModule.OfSeq(multipartFields),
                    TraceHeader.parseTraceHeaders(headers)),
                Http.HttpRuntimeModule.empty, // ToDo
                ConnectionModule.empty , // ToDo
                false); // bool writePreamble What is that? ToDo
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

        public static async Task SetResponseFromSuaveHttpContext(this HttpContext context, Http.HttpContext suaveContext)
        {
            var suaveResponse = suaveContext.response;

            // Set HTTP status code
            context.Response.StatusCode = suaveResponse.status.code;

            // Set HTTP response headers
            foreach (var header in suaveResponse.headers)
            {
                var key = header.Item1;
                var value = header.Item2;

                context.Response.Headers.Add(key, new StringValues(value));
            }

            // Set HTTP body
            if (suaveResponse.content.IsBytes)
            {
                var bytes = ((Http.HttpContent.Bytes)suaveResponse.content).Item;
                context.Response.Headers["Content-Length"] = new StringValues(bytes.Length.ToString());
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            }
            else if (suaveResponse.content.IsSocketTask)
            {
                //ToDo
                throw new NotImplementedException("SocketTask has not been implemeted yet.");
            }
        }
    }
}