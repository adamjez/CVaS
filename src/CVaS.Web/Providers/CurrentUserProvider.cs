using System;
using System.Security.Claims;
using CVaS.BL.Providers;
using CVaS.DAL.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace CVaS.Web.Providers
{
    public class CurrentUserProvider : ICurrentUserProvider
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        public CurrentUserProvider(IHttpContextAccessor contextAccessor, UserManager<AppUser> userManager)
        {
            this._userManager = userManager;
            this._contextAccessor = contextAccessor;
        }

        public int Id => GetId();

        public string UserName => _contextAccessor.HttpContext.User?.Identity?.Name ?? "";

        public ClaimsIdentity ClaimsIdentity => _contextAccessor.HttpContext.User?.Identity as ClaimsIdentity;

        public string DisplayName
        {
            get
            {
                if (ClaimsIdentity == null)
                    throw new UnauthorizedAccessException();

                
                return
                (ClaimsIdentity?.FindFirst(ClaimTypes.GivenName).Value + " " +
                    ClaimsIdentity?.FindFirst(ClaimTypes.Surname).Value).Trim();
                
            }
        }

        public string Email => ClaimsIdentity?.FindFirst(ClaimTypes.Email).Value ?? string.Empty;

        public int? TryGetId
        {
            get
            {
                var user = _userManager.GetUserId(_contextAccessor.HttpContext.User);

                if (user == null)
                    return null;

                return int.Parse(user);
            }
        }

        public bool IsInRole(string roleName)
        {
            return _contextAccessor.HttpContext.User?.IsInRole(roleName) ?? false;
        }

        private int GetId()
        {
            var id = TryGetId;
            if (!id.HasValue)
            {
                throw new UnauthorizedAccessException();
            }
            return id.Value;
        }
    }
}
