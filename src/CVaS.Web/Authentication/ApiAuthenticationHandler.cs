using System.Threading.Tasks;
using CVaS.BL.Common;
using CVaS.BL.Core.Provider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.EntityFrameworkCore;

namespace CVaS.Web.Authentication
{
    public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
    {
        private readonly AppSignInManager _signInManager;
        private readonly AppUserManager _userManager;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public ApiAuthenticationHandler(AppSignInManager signInManager, AppUserManager userManager, IUnitOfWorkProvider unitOfWorkProvider)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authHeader = Context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith(Options.HeaderScheme))
            {
                //Extract credentials
                string apiKey = authHeader.Substring(Options.HeaderScheme.Length).Trim();

                using (_unitOfWorkProvider.Create())
                {
                    var user = await _userManager.Users.FirstOrDefaultAsync(us => us.ApiKey == apiKey);
                    if (user != null)
                    {
                        var principals = await _signInManager.CreateUserPrincipalAsync(user);
                        return AuthenticateResult.Success(
                            new AuthenticationTicket(principals, new AuthenticationProperties(), Options.HeaderScheme));

                    }
                }

                return AuthenticateResult.Fail("bad username or password");
            }

            return AuthenticateResult.Skip();
        }
    }
}