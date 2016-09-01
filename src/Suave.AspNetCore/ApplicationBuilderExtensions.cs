using Microsoft.AspNetCore.Builder;

namespace Suave.AspNetCore
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSuave(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SuaveMiddleware>();
        }
    }
}