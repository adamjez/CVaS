using System.Threading.Tasks;
using CVaS.BL.Services.ApiKey;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;

namespace CVaS.Web.Authentication
{
    public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
    {
        private readonly IApiKeyManager _apiKeyManager;

        public ApiAuthenticationHandler(IApiKeyManager apiKeyManager)
        {
            _apiKeyManager = apiKeyManager;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authHeader = Context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith(Options.HeaderScheme))
            {
                //Extract credentials
                string apiKey = authHeader.Substring(Options.HeaderScheme.Length).Trim();

                var principals = await _apiKeyManager.GetClaimsPrincipalAsync(apiKey);

                if (principals != null)
                {
                    return AuthenticateResult.Success(
                                new AuthenticationTicket(principals, new AuthenticationProperties(), Options.HeaderScheme));
                }

                return AuthenticateResult.Fail("bad username or password");
            }

            return AuthenticateResult.Skip();
        }
    }
}