using System.Security.Claims;
using System.Threading.Tasks;
using CVaS.BL.Services.ApiKey;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CVaS.BL.Common
{
    public class ApiKeyManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly UserRepository _userRepository;
        private readonly IApiKeyGenerator _apiKeyGenerator;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public ApiKeyManager(IMemoryCache memoryCache, IUnitOfWorkProvider unitOfWorkProvider, UserRepository userRepository, 
            IApiKeyGenerator apiKeyGenerator, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _memoryCache = memoryCache;
            _unitOfWorkProvider = unitOfWorkProvider;
            _userRepository = userRepository;
            _apiKeyGenerator = apiKeyGenerator;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<string> RevokeAsync(int userId)
        {
            using (var uow = _unitOfWorkProvider.Create())
            {
                var user = await _userRepository.GetById(userId);

                _memoryCache.Remove(user.ApiKey);

                user.ApiKey = _apiKeyGenerator.Generate();

                await uow.CommitAsync();

                return user.ApiKey;
            }
        }

        public async Task<ClaimsPrincipal> GetClaimsPrincipalAsync(string apiKey)
        {
            using (_unitOfWorkProvider.Create())
            {
                ClaimsPrincipal principals;

                if (!_memoryCache.TryGetValue(apiKey, out principals))
                {
                    var user = await _userManager.Users.FirstOrDefaultAsync(us => us.ApiKey == apiKey);
                    if (user == null)
                    {
                        return null;
                    }

                    principals = await _signInManager.CreateUserPrincipalAsync(user);

                    //// Set cache options.
                    //var cacheEntryOptions = new MemoryCacheEntryOptions()
                    //    // Keep in cache for this time, reset time if accessed.
                    //    .SetSlidingExpiration(TimeSpan.FromMinutes(30));

                    // Save data in cache.
                    _memoryCache.Set(apiKey, principals);
                }

                return principals;
            }
        }
    }
}