# Suave.AspNetCore

[Suave.AspNetCore](https://www.nuget.org/packages/Suave.AspNetCore/) is a small .NET Core library which provides an [ASP.NET Core middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware) to run a [Suave](https://suave.io/) app (on top of [Kestrel](https://github.com/aspnet/KestrelHttpServer)) within ASP.NET Core.

[![Build status](https://ci.appveyor.com/api/projects/status/bj7dxtx4sc8v6x9o/branch/master?svg=true)](https://ci.appveyor.com/project/dustinmoris/suave-aspnetcore/branch/master)
[![NuGet Info](https://buildstats.info/nuget/Suave.AspNetCore)](https://www.nuget.org/packages/Suave.AspNetCore/)

[![Build history](https://buildstats.info/appveyor/chart/dustinmoris/suave-aspnetcore)](https://ci.appveyor.com/project/dustinmoris/suave-aspnetcore/history)

## Current release information

The current version has a dependency on [Suave 2.0.0-rc3](https://www.nuget.org/packages/Suave/2.0.0-rc3), which is not considered a stable release yet. Suave.AspNetCore also only supports Suave's web request handling at the moment and support for web sockets will follow soon.

## Framework support

| Framework | Supported versions | Limitations |
| :--- | :--- | :--- |
| .NET Standard | >= 1.6 | Suave 2.0.0-* requires netstandard1.6 or higher. |
| Full .NET | >= 4.6 | Microsoft.AspNetCore.Http and Microsoft.AspNetCore.WebSockets.Server requires .NET 4.6 or higher. |

## Setup

#### Install NuGet package

```
PM> Install-Package Suave.AspNetCore
```

#### Create a Suave web app

```
open Suave
open Suave.Operators
open Suave.Successful

module App =
    let helloWorld =
        fun (ctx : HttpContext) ->
            OK "Hello World from Suave" ctx
```

#### Add Suave middleware to ASP.NET Core

```
type Startup() =
    member __.Configure (app : IApplicationBuilder)
                        (env : IHostingEnvironment)
                        (loggerFactory : ILoggerFactory) =
        app.UseSuave(App.helloWorld) |> ignore
```

## Additional configuration

#### HTTP header casing

In Suave all HTTP headers are stored in lower case inside the `Suave.Http.HttpRequest` object, but ASP.NET Core preserves the original casing by default.

For example if a client sends the HTTP header `Content-Type: application/json` with the request, then it would be stored as such in ASP.NET Core, but in lower case in Suave: `content-type: application/json`.

When configuring the Suave middleware you can set the `preserveHttpHeaderCasing` parameter to `true` to keep the original casing present (ASP.NET Core style):

```
app.UseSuave(App.helloWorld, true)
```

By default this setting is disabled to match existing Suave applications.

#### Error handling and other Suave config settings

Suave.AspNetCore allows you to hook a Suave web application (`WebPart`) into the ASP.NET Core pipeline. Anything that was in the `SuaveConfig` was web server specific and required to run Suave's own web server via `startWebServer`. In ASP.NET Core there are other means to configure the same settings. For more information please check out the [ASP.NET Core Fundamentals](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/).

## How to build

After forking the project you should be able to run the default .NET CLI commands to build and publish the NuGet package:

```
dotnet restore
dotnet build
dotnet pack
```

## License

[Apache 2.0](https://raw.githubusercontent.com/dustinmoris/Suave.AspNetCore/master/LICENSE)

## Credits

Massive thank you to [ademar](https://github.com/ademar) and [haf](https://github.com/haf) for creating (and open sourcing) Suave in the first place and also a big thanks to [Krzysztof Cieslak](https://github.com/Krzysztof-Cieslak) for open sourcing *a super early alpha version* of [Suave.Kestrel](https://github.com/Krzysztof-Cieslak/Suave.Kestrel) which was as a great kickstarter to get [Suave.AspNetCore](https://www.nuget.org/packages/Suave.AspNetCore/) running.

## Contribution

Feedback is more than welcome and pull requests get accepted!

File an [issue on GitHub](https://github.com/dustinmoris/Suave.AspNetCore/issues/new) or contact me via [https://dusted.codes/about](https://dusted.codes/about).
