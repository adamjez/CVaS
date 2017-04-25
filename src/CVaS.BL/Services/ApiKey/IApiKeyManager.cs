using System.Security.Claims;
using System.Threading.Tasks;

namespace CVaS.BL.Services.ApiKey
{
    public interface IApiKeyManager
    {
        Task RevokeAsync(int userId);
        Task<string> GetApiKey(int userId);
        Task<ClaimsPrincipal> GetClaimsPrincipalAsync(string apiKey);
    }
}