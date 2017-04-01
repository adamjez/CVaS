using System.Security.Claims;
using System.Threading.Tasks;
using CVaS.Shared.Core;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace CVaS.BL.Services.ApiKey
{
    public class CachedApiKeyManager : IApiKeyManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IApiKeyManager _innerApiKeyManager;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly UserRepository _userRepository;

        public CachedApiKeyManager(IMemoryCache memoryCache, IApiKeyManager innerApiKeyManager, IUnitOfWorkProvider unitOfWorkProvider,
            UserRepository userRepository)
        {
            _memoryCache = memoryCache;
            _innerApiKeyManager = innerApiKeyManager;
            _unitOfWorkProvider = unitOfWorkProvider;
            _userRepository = userRepository;
        }

        public async Task RevokeAsync(int userId)
        {
            using (_unitOfWorkProvider.Create(DbContextOptions.DisableTransactionMode))
            {
                var user = await _userRepository.GetById(userId);

                if (user != null)
                {
                    _memoryCache.Remove(user.ApiKey);
                }

                await _innerApiKeyManager.RevokeAsync(userId);
            }
        }

        public Task<string> GetApiKey(int userId)
        {
            return _innerApiKeyManager.GetApiKey(userId);
        }

        public Task<ClaimsPrincipal> GetClaimsPrincipalAsync(string apiKey)
        {
            return _innerApiKeyManager.GetClaimsPrincipalAsync(apiKey);
        }

        public async Task<ClaimsPrincipal> GetClaimsPrincipalAsync(byte[] apiKey)
        {
            if (_memoryCache.TryGetValue(apiKey, out ClaimsPrincipal principals))
            {
                return principals;
            }

            principals = await _innerApiKeyManager.GetClaimsPrincipalAsync(apiKey);

            if (principals != null)
            {
                _memoryCache.Set(apiKey, principals);
            }

            return principals;
        }
    }
}