using System;
using System.Collections.Generic;
using CVaS.BL.Core.Provider;
using CVaS.DAL;
using CVaS.DAL.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.BL.Common
{
    public class AppUserManager : UserManager<AppUser>
    {
        public AppUserManager(AppUserStore userStore, IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<AppUser> passwordHasher, IEnumerable<IUserValidator<AppUser>> userValidators, 
            IEnumerable<IPasswordValidator<AppUser>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<AppUserManager> logger) 
            : base(userStore, optionsAccessor, passwordHasher, userValidators, passwordValidators, 
                keyNormalizer, errors, services, logger)
        {
        }
    }
}