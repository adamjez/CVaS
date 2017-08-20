using System.Threading.Tasks;
using CVaS.BL.Providers;
using CVaS.BL.Services.ApiKey;
using CVaS.BL.Services.Email;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CVaS.BL.Facades
{
    public class AccountFacade : AppFacadeBase
    {
        private readonly IApiKeyGenerator _apiKeyGenerator;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountFacade> _logger;

        public AccountFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider,
            IApiKeyGenerator apiKeyGenerator, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, 
            IEmailSender emailSender, ILogger<AccountFacade> logger) 
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _apiKeyGenerator = apiKeyGenerator;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<CreateUserResult> CreateUserAsync(string email, string password)
        {
            using (UnitOfWorkProvider.Create())
            {
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    ApiKey = _apiKeyGenerator.Generate()
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    return CreateUserResult.FromUser(user, result);
                }

                return CreateUserResult.FromIdentity(result);
            }
        }

        public async Task<SignInResult> SignInAsync(string email, string password)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            using (UnitOfWorkProvider.Create())
            {
                var user = await _userManager.FindByNameAsync(email);

                if (user != null)
                {
                    if (user.EmailConfirmed)
                    {
                        return await _signInManager.PasswordSignInAsync(user, password, false, lockoutOnFailure: false);
                    }

                    return SignInResult.NotAllowed;
                }
            }

            return SignInResult.Failed;
        }
    }

    public struct CreateUserResult
    {
        public AppUser User { get; set; }
        public IdentityResult Identity { get; set; }
        public bool Suceeded => Identity.Succeeded;

        public static CreateUserResult FromIdentity(IdentityResult identity)
        {
            return new CreateUserResult
            {
                Identity = identity
            };
        }

        public static CreateUserResult FromUser(AppUser user, IdentityResult identity)
        {
            return new CreateUserResult
            {
                Identity = identity,
                User = user
            };
        }
    }
}
