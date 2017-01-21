namespace Suave.AspNetCore.App

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Newtonsoft.Json
open Suave
open Suave.Filters
open Suave.Operators
open Suave.DotLiquid
open Suave.Successful
open Suave.AspNetCore
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket

module App =

    type ViewModel = 
        {
            Message : string
        }   

    DotLiquid.setTemplatesDir (__SOURCE_DIRECTORY__ + "./Views")

    let dotLiquidHandler = 
        path "/dotliquid" 
        >=> DotLiquid.page "index.html" { Message = "Hello World from a DotLiquid template." }

    let catchAll =
        fun (ctx : HttpContext) ->
            let json = JsonConvert.SerializeObject(ctx.request, Formatting.Indented)
            OK json 
            >=> Writers.setMimeType "application/json"
            <| ctx
    
    let app =
        choose [
            dotLiquidHandler
            catchAll
            ]

type Startup() =
    member __.Configure (app : IApplicationBuilder)
                        (env : IHostingEnvironment)
                        (loggerFactory : ILoggerFactory) =
        app.UseSuave(App.app) |> ignore