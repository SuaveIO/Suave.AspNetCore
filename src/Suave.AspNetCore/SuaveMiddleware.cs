using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

using SuaveApplication = 
    Microsoft.FSharp.Core.FSharpFunc<
        Suave.Http.HttpContext, 
        Microsoft.FSharp.Control.FSharpAsync<
            Microsoft.FSharp.Core.FSharpOption<Suave.Http.HttpContext>>>;

namespace Suave.AspNetCore
{
    public class SuaveMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SuaveApplication _app;
        private readonly bool _preserveHttpHeaderCasing;

        public SuaveMiddleware(
            RequestDelegate next,
            SuaveApplication app,
            bool preserveHttpHeaderCasing)
        {
            _next = next;
            _app = app;
            _preserveHttpHeaderCasing = preserveHttpHeaderCasing;
        }

        public async Task Invoke(HttpContext context)
        {
            var suaveContext = await context.ToSuaveHttpContext(_preserveHttpHeaderCasing);
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