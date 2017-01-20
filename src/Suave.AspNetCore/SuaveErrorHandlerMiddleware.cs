using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

namespace Suave.AspNetCore
{
    public sealed class SuaveErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly FSharpFunc<Exception, FSharpFunc<string, FSharpFunc<Http.HttpContext, FSharpAsync<FSharpOption<Http.HttpContext>>>>> _errorHandler;

        public SuaveErrorHandlerMiddleware(
            RequestDelegate next, 
            ILoggerFactory loggerFactory,
            FSharpFunc<Exception, FSharpFunc<string, FSharpFunc<Http.HttpContext, FSharpAsync<FSharpOption<Http.HttpContext>>>>> errorHandler)
        {
            
            _next = next;
            _errorHandler = errorHandler;
            _logger = loggerFactory.CreateLogger<SuaveErrorHandlerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception)
            {
                try
                {
                    // Do custom stuff
                    // Could be just as simple as calling _logger.LogError

                    // if you don't want to rethrow the original exception
                    // then call return:
                    // return;
                }
                catch (Exception ex2)
                {
                    _logger.LogError(
                        0, ex2,
                        "An exception was thrown attempting " +
                        "to execute the error handler.");
                }

                // Otherwise this handler will
                // re -throw the original exception
                throw;
            }
        }
    }
}