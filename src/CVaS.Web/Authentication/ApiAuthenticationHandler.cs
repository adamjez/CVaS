using System;
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
        private readonly Func<AppSignInManager> _signInManagerFactory;
        private readonly Func<AppUserManager> _userManagerFactory;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public ApiAuthenticationHandler(Func<AppSignInManager> signInManagerFactory, Func<AppUserManager> userManagerFactory, IUnitOfWorkProvider unitOfWorkProvider)
        {
            _signInManagerFactory = signInManagerFactory;
            _userManagerFactory = userManagerFactory;
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
                    var userManager = _userManagerFactory();
               
                    var user = await userManager.Users.FirstOrDefaultAsync(us => us.ApiKey == apiKey);
                    if (user != null)
                    {
                        var principals = await _signInManagerFactory().CreateUserPrincipalAsync(user);
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