using System;
using Microsoft.Extensions.DependencyInjection;

namespace CVaS.Web.Authentication
{
    public static class ApiAuthenticationServiceCollectionExtensions
    {
        public static void AddApiAuthentication(this IServiceCollection services, Action<ApiAuthenticationOptions> setupAction)
        {
            services.Configure(setupAction);
        }
    }
}