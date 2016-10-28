﻿namespace CVaS.BL.Providers
{
    public interface ICurrentUserProvider
    {
        int? TryGetId { get; }

        int Id { get; }

        string UserName { get; }

        string DisplayName { get; }

        string Email { get; }

        bool IsInRole(string roleName);
    }
}