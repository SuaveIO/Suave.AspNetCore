using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.FSharp.Collections;

namespace Suave.AspNetCore
{
    public static class HttpContextExtensions
    {
        public static Http.HttpContext ToSuaveHttpContext(this HttpContext context)
        {
            // Get HTTP headers
            var headers = 
                ListModule.OfSeq(
                    context.Request.Headers
                        .Select(h => new Tuple<string, string>(h.Key, h.Value))
                        .ToList());
            
            return Http.HttpContextModule.create(
                new Http.HttpRequest(
                    context.Request.Protocol,
                    new Uri(""),
                    context.Request.Host.Value,
                    HttpMethodFromString(context.Request.Method),
                    headers,
                    null, // byte[] rawForm
                    "rawQuery",
                    null, // FSharpList<HttpUpload> files,
                    null, // FSharpList<Tuple<string,string>> multiPartFields
                    null), // TraceHeader trace
                null, // HttpRuntime
                null , // Connection
                false); // bool writePreamble
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