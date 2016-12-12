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

        public SuaveMiddleware(RequestDelegate next, FSharpFunc<Http.HttpContext, FSharpAsync<FSharpOption<Http.HttpContext>>> app)
        {
            _next = next;
            _app = app;
        }

        public async Task Invoke(HttpContext context)
        {
            var suaveRequest = await context.Request.ToSuaveHttpRequest();
            var suaveContext = 
                Http.HttpContextModule.create(
                    suaveRequest,
                    // Runtime settings to be set via middleware in ASP.NET Core
                    Http.HttpRuntimeModule.empty,
                    // ToDo
                    ConnectionModule.empty,
                    false);
            var asyncWorkflow = _app.Invoke(suaveContext);
            var result = await FSharpAsync.StartAsTask(
                asyncWorkflow,
                FSharpOption<TaskCreationOptions>.Some(TaskCreationOptions.None),
                FSharpOption<CancellationToken>.Some(CancellationToken.None));

            await context.SetResponseFromSuaveHttpContext(result.Value);

            await _next.Invoke(context);
        }
    }
}