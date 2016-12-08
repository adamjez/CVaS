using System;
using System.Linq;
using System.Threading.Tasks;
using CVaS.BL.Core.Provider;
using CVaS.DAL.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CVaS.Web.Authentication
{
    public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
    {
        private const string Scheme = "Simple";
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public ApiAuthenticationHandler(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IUnitOfWorkProvider unitOfWorkProvider)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authHeader = Context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith(Scheme))
            {
                //Extract credentials
                string apiKey = authHeader.Substring(Scheme.Length).Trim();

                //Guid apiKeyGuid;
                //if (Guid.TryParse(apiKey, out apiKeyGuid))
                {
                    using (_unitOfWorkProvider.Create())
                    {
                        var user = await _userManager.Users.FirstOrDefaultAsync(us => us.ApiKey == apiKey);
                        if (user != null)
                        {
                            var principals = await _signInManager.CreateUserPrincipalAsync(user);
                            return AuthenticateResult.Success(
                                new AuthenticationTicket(principals, new AuthenticationProperties(), Scheme));
                        }
                    }

                    return AuthenticateResult.Fail("bad username or password");
                }

            }

            return AuthenticateResult.Fail("Missing authentication header or bad authorization type");
        }
    }
}