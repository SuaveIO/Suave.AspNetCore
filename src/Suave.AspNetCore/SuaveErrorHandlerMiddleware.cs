using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

using SuaveErrorHandler = 
    Microsoft.FSharp.Core.FSharpFunc<
        System.Exception, 
        Microsoft.FSharp.Core.FSharpFunc<
            string, 
            Microsoft.FSharp.Core.FSharpFunc<
                Suave.Http.HttpContext, 
                Microsoft.FSharp.Control.FSharpAsync<
                    Microsoft.FSharp.Core.FSharpOption<Suave.Http.HttpContext>>>>>;

namespace Suave.AspNetCore
{
    public sealed class SuaveErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly SuaveErrorHandler _errorHandler;
        private readonly bool _preserveHttpHeaderCasing;

        public SuaveErrorHandlerMiddleware(
            RequestDelegate next, 
            ILoggerFactory loggerFactory,
            SuaveErrorHandler errorHandler,
            bool preserveHttpHeaderCasing)
        {
            
            _next = next;
            _errorHandler = errorHandler;
            _preserveHttpHeaderCasing = preserveHttpHeaderCasing;
            _logger = loggerFactory.CreateLogger<SuaveErrorHandlerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                try
                {
                    var suaveContext = await context.ToSuaveHttpContext(_preserveHttpHeaderCasing);
                    var asyncWorkflow = _errorHandler.Invoke(ex).Invoke("").Invoke(suaveContext);

                    var result = await FSharpAsync.StartAsTask(
                        asyncWorkflow,
                        FSharpOption<TaskCreationOptions>.Some(TaskCreationOptions.None),
                        FSharpOption<CancellationToken>.Some(CancellationToken.None));

                    // Set the Suave result if the request could be handled by Suave and return
                    if (FSharpOption<Http.HttpContext>.get_IsSome(result))
                    {
                        await context.SetResponseFromSuaveResult(result.Value.response);
                        return;
                    }
                }
                catch (Exception ex2)
                {
                    _logger.LogError(0, ex, $"An unhandled exception has occured: {ex.Message}.");
                    _logger.LogError(0, ex2, $"An exception was thrown attempting to execute the error handler: {ex2.Message}.");
                }

                // Re -throw the original exception
                throw;
            }
        }
    }
}