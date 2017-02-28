using System;
using System.Collections.Generic;
using CVaS.DAL.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.BL.Common
{
    public class AppUserManager : UserManager<AppUser>
    {
        public AppUserManager(IOptions<IdentityOptions> optionsAccessor, IUserStore<AppUser> appUserStore,
            IPasswordHasher<AppUser> passwordHasher, IEnumerable<IUserValidator<AppUser>> userValidators,
            IEnumerable<IPasswordValidator<AppUser>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<AppUserManager> logger)
            : base(appUserStore, optionsAccessor, passwordHasher, userValidators, passwordValidators,
                keyNormalizer, errors, services, logger)
        {
        }
    }
}