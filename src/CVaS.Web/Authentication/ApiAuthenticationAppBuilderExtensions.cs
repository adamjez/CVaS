using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CVaS.Web.Authentication
{
    public static class ApiAuthenticationAppBuilderExtensions
    {
        public static IApplicationBuilder UseApiAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            var options = app.ApplicationServices.GetRequiredService<IOptions<ApiAuthenticationOptions>>().Value;

            return app.UseMiddleware<ApiAuthenticationMiddleware>(options);
        }

        public static IApplicationBuilder UseApiAuthentication(this IApplicationBuilder app, ApiAuthenticationOptions options)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            return app.UseMiddleware<ApiAuthenticationMiddleware>(Options.Create(options));
        }
    }
}