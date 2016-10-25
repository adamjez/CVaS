using System;
using Microsoft.AspNetCore.Builder;

namespace CVaS.Web.Authentication
{
    public static class CustomAuthenticationAppBuilderExtensions
    {
        public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            return app.UseMiddleware<CustomAuthenticationMiddleware>(Array.Empty<object>());
        }
    }
}