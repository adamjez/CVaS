using System;
using System.Linq;
using System.Security.Cryptography;

namespace CVaS.BL.Services.ApiKey
{
    public class RndApiKeyGenerator : IApiKeyGenerator
    {
        public string Generate()
        {
            var key = new byte[32];
            using (var generator = RandomNumberGenerator.Create())
                generator.GetBytes(key);
            return Convert.ToBase64String(key);
        }
    }
}