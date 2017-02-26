using System;

namespace CVaS.Shared.Helpers
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