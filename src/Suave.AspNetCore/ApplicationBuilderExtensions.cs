using Microsoft.AspNetCore.Builder;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

namespace Suave.AspNetCore
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSuave(
            this IApplicationBuilder builder, 
            FSharpFunc<Http.HttpContext, FSharpAsync<FSharpOption<Http.HttpContext>>> app)
        {
            return builder.UseMiddleware<SuaveMiddleware>(app);
        }
    }
}