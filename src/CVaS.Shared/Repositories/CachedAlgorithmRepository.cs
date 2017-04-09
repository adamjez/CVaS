using System.Threading.Tasks;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Helpers;
using Microsoft.Extensions.Caching.Memory;

namespace CVaS.Shared.Repositories
{
    public class CachedAlgorithmRepository : AlgorithmRepository
    {
        private readonly IMemoryCache _memoryCache;

        public CachedAlgorithmRepository(IUnitOfWorkProvider provider, IMemoryCache memoryCache) : base(provider)
        {
            _memoryCache = memoryCache;
        }

        public override async Task<Algorithm> GetByCodeNameWithArgs(string codeName)
        {
            if (_memoryCache.TryGetValue(codeName, CacheType.AlgorithmRepository, out Algorithm algorithm))
            {
                return algorithm;
            }

            algorithm = await base.GetByCodeNameWithArgs(codeName);

            _memoryCache.Set(algorithm.CodeName, CacheType.AlgorithmRepository, algorithm);

            return algorithm;
        }
    }
}