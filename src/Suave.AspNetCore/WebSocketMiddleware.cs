using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

namespace Suave.AspNetCore
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly FSharpFunc<System.Net.WebSockets.WebSocket, FSharpAsync<Unit>> _app;

        public WebSocketMiddleware(
            RequestDelegate next,
            FSharpFunc<System.Net.WebSockets.WebSocket, FSharpAsync<Unit>> app)
        {
            _next = next;
            _app = app;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                var asyncWorkflow = _app.Invoke(webSocket);

                await FSharpAsync.StartAsTask(
                    asyncWorkflow,
                    FSharpOption<TaskCreationOptions>.Some(TaskCreationOptions.None),
                    FSharpOption<CancellationToken>.Some(CancellationToken.None));
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}