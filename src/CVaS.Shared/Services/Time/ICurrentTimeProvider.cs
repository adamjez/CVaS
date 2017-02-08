using System;

namespace CVaS.Shared.Services.Time
{
    public interface ICurrentTimeProvider
    {
        DateTime Now();
    }

    public class UtcNowTimeProvider : ICurrentTimeProvider
    {
        public DateTime Now() => DateTime.UtcNow;
    }
}