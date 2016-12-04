using System;
using Microsoft.AspNetCore.Builder;

namespace CVaS.Web.Authentication
{
    public static class BasicAuthenticationAppBuilderExtensions
    {
        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            return app.UseMiddleware<BasicAuthenticationMiddleware>(Array.Empty<object>());
        }
    }
}