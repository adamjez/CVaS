﻿using System;
using System.Security.Claims;
using CVaS.DAL.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using CVaS.BL.Providers;

namespace CVaS.Web.Providers
{
    /// <summary>
    /// Providers currently logger user and
    /// his informations
    /// </summary>
    public class CurrentUserProvider : ICurrentUserProvider
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        public CurrentUserProvider(IHttpContextAccessor contextAccessor, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _contextAccessor = contextAccessor;
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

        public bool Exists => _contextAccessor.HttpContext.User.Identity?.IsAuthenticated ?? false;

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

        public ClaimsPrincipal GetClaims()
        {
            return _contextAccessor.HttpContext.User;
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
