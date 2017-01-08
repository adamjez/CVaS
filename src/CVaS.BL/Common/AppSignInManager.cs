﻿using System;
using CVaS.DAL.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.BL.Common
{
    public class AppSignInManager : SignInManager<AppUser>
    {
        public AppSignInManager(AppUserManager userManager, IHttpContextAccessor contextAccessor, 
            IUserClaimsPrincipalFactory<AppUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, 
            ILogger<AppSignInManager> logger)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger)
        {
        }
    }
}
