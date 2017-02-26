using System;
using System.Text.Encodings.Web;
using CVaS.BL.Common;
using CVaS.Shared.Core.Provider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.Web.Authentication
{
    public class ApiAuthenticationMiddleware : AuthenticationMiddleware<ApiAuthenticationOptions>
    {
        private readonly AppSignInManager _signInManager;
        private readonly AppUserManager _userManager;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public ApiAuthenticationMiddleware(RequestDelegate next, AppSignInManager signInManager,
            ILoggerFactory loggerFactory, UrlEncoder urlEncoder, IOptions<ApiAuthenticationOptions> options,
            AppUserManager userManager, IUnitOfWorkProvider unitOfWorkProvider)
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