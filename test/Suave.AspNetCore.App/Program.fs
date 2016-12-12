// Learn more about F# at http://fsharp.org

open System
open System.IO
open Microsoft.AspNetCore.Hosting

[<EntryPoint>]
let main argv = 
    let host =
        WebHostBuilder()
            .UseKestrel()
            .UseStartup<Suave.AspNetCore.App.Startup>()
            .Build()
    host.Run()
    0