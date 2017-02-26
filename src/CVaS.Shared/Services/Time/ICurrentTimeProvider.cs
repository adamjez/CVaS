using System;

namespace CVaS.Shared.Services.Time
{
    public interface ICurrentTimeProvider
    {
        DateTime Now();
    }
}