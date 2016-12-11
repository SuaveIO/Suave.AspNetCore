namespace AspNetCoreFSharp

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Suave.AspNetCore

type Startup() =
    member __.Configure (app : IApplicationBuilder)
                        (env : IHostingEnvironment)
                        (loggerFactory : ILoggerFactory) =

        app.UseSuave(App.endpoint) |> ignore

        //app.Run(fun ctx -> ctx.Response.WriteAsync("Hello World!"))