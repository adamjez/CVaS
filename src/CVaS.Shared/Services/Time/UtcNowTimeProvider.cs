﻿using System;

namespace CVaS.Shared.Services.Time
{
    public class UtcNowTimeProvider : ICurrentTimeProvider
    {
        public DateTime Now() => DateTime.UtcNow;
    }
}