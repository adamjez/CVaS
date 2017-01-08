using System;

namespace CVaS.BL.Helpers
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