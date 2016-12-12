namespace Suave.AspNetCore.App

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Newtonsoft.Json
open Suave
open Suave.Operators
open Suave.Successful
open Suave.AspNetCore

module App =
    let catchAll =
        fun (ctx : HttpContext) ->
            let json = JsonConvert.SerializeObject(ctx.request, Formatting.Indented)
            OK json 
            >=> Writers.setMimeType "application/json"
            <| ctx

type Startup() =
    member __.Configure (app : IApplicationBuilder)
                        (env : IHostingEnvironment)
                        (loggerFactory : ILoggerFactory) =
        app.UseSuave(App.catchAll) |> ignore