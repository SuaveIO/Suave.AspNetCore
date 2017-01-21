using Microsoft.AspNetCore.Builder;

using SuaveApplication =
    Microsoft.FSharp.Core.FSharpFunc<
        Suave.Http.HttpContext,
        Microsoft.FSharp.Control.FSharpAsync<
            Microsoft.FSharp.Core.FSharpOption<Suave.Http.HttpContext>>>;

using SuaveErrorHandler =
    Microsoft.FSharp.Core.FSharpFunc<
        System.Exception,
        Microsoft.FSharp.Core.FSharpFunc<
            string,
            Microsoft.FSharp.Core.FSharpFunc<
                Suave.Http.HttpContext,
                Microsoft.FSharp.Control.FSharpAsync<
                    Microsoft.FSharp.Core.FSharpOption<Suave.Http.HttpContext>>>>>;

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
            SuaveApplication app,
            bool preserveHttpHeaderCasing = false)
        {
            return builder.UseMiddleware<SuaveMiddleware>(app, preserveHttpHeaderCasing);
        }

        /// <summary>
        /// Adds Suave middleware to the application's request pipeline.
        /// </summary>
        /// <param name="builder">Application builder object</param>
        /// <param name="errorHandler">Suave ErrorHandler</param>
        /// <param name="preserveHttpHeaderCasing">Denotes whether the HTTP headers in the Suave.Http.HttpRequest object preserve the original casing or not (defaults to false)</param>
        public static IApplicationBuilder UseSuaveErrorHandler(
            this IApplicationBuilder builder,
            SuaveErrorHandler errorHandler,
            bool preserveHttpHeaderCasing = false)
        {
            return builder.UseMiddleware<SuaveErrorHandlerMiddleware>(errorHandler, preserveHttpHeaderCasing);
        }
    }
}