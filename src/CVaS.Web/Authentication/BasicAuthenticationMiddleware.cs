using System.Text.Encodings.Web;
using CVaS.DAL.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.Web.Authentication
{
    public class BasicAuthenticationMiddleware : AuthenticationMiddleware<BasicAuthenticationOptions>
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public BasicAuthenticationMiddleware(RequestDelegate next, SignInManager<AppUser> signInManager,
            ILoggerFactory loggerFactory, UrlEncoder urlEncoder, IOptions<BasicAuthenticationOptions> options,
            UserManager<AppUser> userManager)
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