using CVaS.DAL.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.BL.Common
{
    public class AppSignInManager : SignInManager<AppUser>
    {
        public AppSignInManager(UserManager<AppUser> userManager, IHttpContextAccessor contextAccessor, 
            IUserClaimsPrincipalFactory<AppUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, 
            ILogger<AppSignInManager> logger, IAuthenticationSchemeProvider authenticationSchemeProvider)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, authenticationSchemeProvider)
        {
        }
    }
}
