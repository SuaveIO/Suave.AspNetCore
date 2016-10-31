using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

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
            var suaveContext = context.ToSuaveHttpContext();
            var asyncWorkflow = _app.Invoke(suaveContext);
            var result = await FSharpAsync.StartAsTask(
                asyncWorkflow,
                FSharpOption<TaskCreationOptions>.Some(TaskCreationOptions.DenyChildAttach),
                FSharpOption<CancellationToken>.Some(CancellationToken.None));

            await context.SetResponseFromSuaveHttpContext(result.Value);

            await _next.Invoke(context);
        }
    }
}