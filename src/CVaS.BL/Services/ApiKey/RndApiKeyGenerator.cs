using System.Security.Cryptography;

namespace CVaS.BL.Services.ApiKey
{
    public class RndApiKeyGenerator : IApiKeyGenerator
    {
        public byte[] Generate()
        {
            var key = new byte[32];
            using (var generator = RandomNumberGenerator.Create())
                generator.GetBytes(key);
            return key;
        }
    }
}