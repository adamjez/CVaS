using System;
using System.Text.Encodings.Web;
using CVaS.BL.Common;
using CVaS.BL.Core.Provider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.Web.Authentication
{
    public class ApiAuthenticationMiddleware : AuthenticationMiddleware<ApiAuthenticationOptions>
    {
        private readonly Func<AppSignInManager> _signInManagerFactory;
        private readonly Func<AppUserManager> _userManagerFactory;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public ApiAuthenticationMiddleware(RequestDelegate next, Func<AppSignInManager> signInManagerFactory,
            ILoggerFactory loggerFactory, UrlEncoder urlEncoder, IOptions<ApiAuthenticationOptions> options,
            Func<AppUserManager> userManagerFactory, IUnitOfWorkProvider unitOfWorkProvider)
            : base(next, options, loggerFactory, urlEncoder)
        {
            _signInManagerFactory = signInManagerFactory;
            _userManagerFactory = userManagerFactory;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        protected override AuthenticationHandler<ApiAuthenticationOptions> CreateHandler()
        {
            return new ApiAuthenticationHandler(_signInManagerFactory, _userManagerFactory, _unitOfWorkProvider);
        }
    }
}