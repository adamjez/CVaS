using System.Security.Claims;

namespace CVaS.Shared.Providers
{
    public interface ICurrentUserProvider
    {
        bool Exists { get; }

        int? TryGetId { get; }

        int Id { get; }

        string UserName { get; }

        string DisplayName { get; }

        string Email { get; }

        bool IsInRole(string roleName);

        ClaimsPrincipal GetClaims();
    }
}
