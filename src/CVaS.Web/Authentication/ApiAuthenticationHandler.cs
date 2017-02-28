using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CVaS.BL.Common;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CVaS.Web.Authentication
{
    public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IMemoryCache _cache;

        public ApiAuthenticationHandler(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IUnitOfWorkProvider unitOfWorkProvider, IMemoryCache cache)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _unitOfWorkProvider = unitOfWorkProvider;
            _cache = cache;
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
                    ClaimsPrincipal principals;

                    if (!_cache.TryGetValue(apiKey, out principals))
                    {
                        var user = await _userManager.Users.FirstOrDefaultAsync(us => us.ApiKey == apiKey);
                        if (user != null)
                        {
                            principals = await _signInManager.CreateUserPrincipalAsync(user);

                            // Set cache options.
                            var cacheEntryOptions = new MemoryCacheEntryOptions()
                                // Keep in cache for this time, reset time if accessed.
                                .SetSlidingExpiration(TimeSpan.FromMinutes(30));

                            // Save data in cache.
                            _cache.Set(apiKey, principals, cacheEntryOptions);
                        }
                    }

                    if (principals != null)
                        return AuthenticateResult.Success(
                            new AuthenticationTicket(principals, new AuthenticationProperties(), Options.HeaderScheme));

                }

                return AuthenticateResult.Fail("bad username or password");
            }

            return AuthenticateResult.Skip();
        }
    }
}