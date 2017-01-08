using System.Text.Encodings.Web;
using CVaS.BL.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.Web.Authentication
{
    public class BasicAuthenticationMiddleware : AuthenticationMiddleware<BasicAuthenticationOptions>
    {
        private readonly AppSignInManager _signInManager;
        private readonly AppUserManager _userManager;

        public BasicAuthenticationMiddleware(RequestDelegate next, AppSignInManager signInManager,
            ILoggerFactory loggerFactory, UrlEncoder urlEncoder, IOptions<BasicAuthenticationOptions> options,
            AppUserManager userManager)
            : base(next, options, loggerFactory, urlEncoder)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        protected override AuthenticationHandler<BasicAuthenticationOptions> CreateHandler()
        {
            return new BasicAuthenticationHandler(_signInManager, _userManager);
        }
    }
}