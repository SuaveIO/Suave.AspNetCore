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
                    "HTTP VERSION",
                    new Uri(""),
                    "host",
                    Http.HttpMethod.GET,
                    headers,
                    null,
                    "rawQuery",
                    null,
                    null,
                    null),
                null,
                null,
                false);
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