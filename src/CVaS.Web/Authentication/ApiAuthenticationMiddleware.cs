using System.Text.Encodings.Web;
using CVaS.BL.Core.Provider;
using CVaS.DAL.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.Web.Authentication
{
    public class ApiAuthenticationMiddleware : AuthenticationMiddleware<ApiAuthenticationOptions>
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public ApiAuthenticationMiddleware(RequestDelegate next, SignInManager<AppUser> signInManager,
            ILoggerFactory loggerFactory, UrlEncoder urlEncoder, IOptions<ApiAuthenticationOptions> options,
            UserManager<AppUser> userManager, IUnitOfWorkProvider unitOfWorkProvider)
            : base(next, options, loggerFactory, urlEncoder)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        protected override AuthenticationHandler<ApiAuthenticationOptions> CreateHandler()
        {
            return new ApiAuthenticationHandler(_signInManager, _userManager, _unitOfWorkProvider);
        }
    }
}