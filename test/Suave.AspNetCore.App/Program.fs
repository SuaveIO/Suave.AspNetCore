open System
open System.IO
open Microsoft.AspNetCore.Hosting
open Suave.AspNetCore.App

[<EntryPoint>]
let main argv = 
    let host =
        WebHostBuilder()
            .UseKestrel()
            .UseStartup<Startup>()
            .Build()
    host.Run()
    0