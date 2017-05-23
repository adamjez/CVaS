using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CVaS.BL.Services.ApiKey
{
    public class ApiKeyManager : IApiKeyManager
    {
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly UserRepository _userRepository;
        private readonly IApiKeyGenerator _apiKeyGenerator;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public ApiKeyManager(IUnitOfWorkProvider unitOfWorkProvider, UserRepository userRepository,
            IApiKeyGenerator apiKeyGenerator, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _unitOfWorkProvider = unitOfWorkProvider;
            _userRepository = userRepository;
            _apiKeyGenerator = apiKeyGenerator;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task RevokeAsync(int userId)
        {
            using (var uow = _unitOfWorkProvider.Create())
            {
                var user = await _userRepository.GetById(userId);

                user.ApiKey = _apiKeyGenerator.Generate();

                await uow.CommitAsync();
            }
        }

        public async Task<string> GetApiKey(int userId)
        {
            using (_unitOfWorkProvider.Create())
            {
                var user = await _userRepository.GetById(userId);

                return Convert.ToBase64String(user.ApiKey);
            }
        }

        public async Task<ClaimsPrincipal> GetClaimsPrincipalAsync(string apiKey)
        {
            try
            {
                return await GetClaimsPrincipalAsync(Convert.FromBase64String(apiKey));
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private async Task<ClaimsPrincipal> GetClaimsPrincipalAsync(byte[] apiKey)
        {
            using (_unitOfWorkProvider.Create())
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(us => us.ApiKey == apiKey);
                if (user == null)
                {
                    return null;
                }

                return await _signInManager.CreateUserPrincipalAsync(user);
            }
        }
    }
}