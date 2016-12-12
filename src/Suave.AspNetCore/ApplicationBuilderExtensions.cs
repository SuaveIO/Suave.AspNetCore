using Microsoft.AspNetCore.Builder;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

namespace Suave.AspNetCore
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds Suave middleware to the application's request pipeline.
        /// </summary>
        /// <param name="builder">Application builder object</param>
        /// <param name="app">Suave WebPart (see: https://suave.io/composing.html)</param>
        /// <param name="preserveHttpHeaderCasing">Denotes whether the HTTP headers in the Suave.Http.HttpRequest object preserve the original casing or not (defaults to false)</param>
        public static IApplicationBuilder UseSuave(
            this IApplicationBuilder builder, 
            FSharpFunc<Http.HttpContext, FSharpAsync<FSharpOption<Http.HttpContext>>> app,
            bool preserveHttpHeaderCasing = false)
        {
            return builder.UseMiddleware<SuaveMiddleware>(app, preserveHttpHeaderCasing);
        }
    }
}