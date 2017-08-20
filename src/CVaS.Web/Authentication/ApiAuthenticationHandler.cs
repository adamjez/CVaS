using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CVaS.BL.Services.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.Web.Authentication
{
    public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
    {
        private readonly IApiKeyManager _apiKeyManager;

        public ApiAuthenticationHandler(IOptionsMonitor<ApiAuthenticationOptions> options, ILoggerFactory logger, 
            UrlEncoder encoder, ISystemClock clock, IApiKeyManager apiKeyManager) 
            : base(options, logger, encoder, clock)
        {
            _apiKeyManager = apiKeyManager;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authHeader = Context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith(Options.HeaderScheme))
            {
                // Extract api key
                string apiKey = authHeader.Substring(Options.HeaderScheme.Length).Trim();

                var principals = await _apiKeyManager.GetClaimsPrincipalAsync(apiKey);

                if (principals != null)
                {
                    return AuthenticateResult.Success(
                                new AuthenticationTicket(principals, Options.HeaderScheme));
                }

                return AuthenticateResult.Fail("Bad username or password");
            }

            return AuthenticateResult.NoResult();
        }
    }
}
