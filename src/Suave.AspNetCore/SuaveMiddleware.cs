using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Suave.Sockets;

namespace Suave.AspNetCore
{
    public class SuaveMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly FSharpFunc<Http.HttpContext, FSharpAsync<FSharpOption<Http.HttpContext>>> _app;
        private readonly bool _preserveHttpHeaderCasing;

        public SuaveMiddleware(
            RequestDelegate next, 
            FSharpFunc<Http.HttpContext, FSharpAsync<FSharpOption<Http.HttpContext>>> app,
            bool preserveHttpHeaderCasing = false)
        {
            _next = next;
            _app = app;
            _preserveHttpHeaderCasing = preserveHttpHeaderCasing;
        }

        public async Task Invoke(HttpContext context)
        {
            var suaveRequest = await context.Request.ToSuaveHttpRequest(_preserveHttpHeaderCasing);

            // Runtime settings to be set via middleware in ASP.NET Core
            var suaveRuntime = Http.HttpRuntimeModule.empty;

            // ToDo
            var suaveSocketConnection = ConnectionModule.empty;

            var suaveContext = 
                Http.HttpContextModule.create(
                    suaveRequest,
                    suaveRuntime,
                    suaveSocketConnection,
                    false);

            var asyncWorkflow = _app.Invoke(suaveContext);

            var result = await FSharpAsync.StartAsTask(
                asyncWorkflow,
                FSharpOption<TaskCreationOptions>.Some(TaskCreationOptions.None),
                FSharpOption<CancellationToken>.Some(CancellationToken.None));

            // Set the Suave result if the request could be handled by Suave
            if (FSharpOption<Http.HttpContext>.get_IsSome(result))
            {
                await context.SetResponseFromSuaveResult(result.Value.response);
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}