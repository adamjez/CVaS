using System.Text.Encodings.Web;
using CVaS.BL.Common;
using CVaS.BL.Services.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.Web.Authentication
{
    public class ApiAuthenticationMiddleware : AuthenticationMiddleware<ApiAuthenticationOptions>
    {
        private readonly IApiKeyManager _apiKeyManager;

        public ApiAuthenticationMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, UrlEncoder urlEncoder, 
            IOptions<ApiAuthenticationOptions> options, IApiKeyManager apiKeyManager)
            : base(next, options, loggerFactory, urlEncoder)
        {
            _apiKeyManager = apiKeyManager;
        }

        protected override AuthenticationHandler<ApiAuthenticationOptions> CreateHandler()
        {
            return new ApiAuthenticationHandler(_apiKeyManager);
        }
    }
}