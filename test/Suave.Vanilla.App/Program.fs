open System
open Suave
open Suave.Filters
open Suave.Successful
open Suave.Operators
open Suave.DotLiquid
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket
open Newtonsoft.Json

//
// DotLiquid
//
type ViewModel = 
    {
        Message : string
    }   

DotLiquid.setTemplatesDir (__SOURCE_DIRECTORY__ + "./Views")

let dotLiquidHandler = 
    path "/dotliquid" 
    >=> DotLiquid.page "index.html" { Message = "Hello World from a DotLiquid template." }

//
// WebSockets
//
let echo (webSocket : WebSocket) =
    fun ctx ->
        socket {
            let loop = ref true
            while !loop do
                let! msg = webSocket.read()
                match msg with
                | (Text, data, true) ->
                    let str = UTF8.toString data
                    do! webSocket.send Text (ArraySegment data) true
                | (Ping, _, _) ->
                    do! webSocket.send Pong (ArraySegment([||])) true
                | (Close, _, _) ->
                    do! webSocket.send Close (ArraySegment([||])) true
                    loop := false
                | _ -> ()
        }

//
// Everything else
//
let catchAll =
    fun (ctx : HttpContext) ->
        let json = JsonConvert.SerializeObject(ctx.request, Formatting.Indented)
        OK json 
        >=> Writers.setMimeType "application/json"
        <| ctx

//
// App
//
let app =
    choose [
        path "/websocket" >=> handShake echo
        dotLiquidHandler
        catchAll
        ]

[<EntryPoint>]
let main argv = 
    startWebServer defaultConfig app
    0