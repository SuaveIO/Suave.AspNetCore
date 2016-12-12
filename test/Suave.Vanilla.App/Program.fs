open System
open Suave
open Suave.Successful
open Suave.Operators
open Newtonsoft.Json

let catchAll =
    fun (ctx : HttpContext) ->
        let json = JsonConvert.SerializeObject(ctx.request, Formatting.Indented)
        OK json 
        >=> Writers.setMimeType "application/json"
        <| ctx

[<EntryPoint>]
let main argv = 
    startWebServer defaultConfig catchAll
    0