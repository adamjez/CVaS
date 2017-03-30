using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using Microsoft.Extensions.Caching.Memory;

namespace CVaS.Shared.Repositories
{
    public class CachedFileRepository : FileRepository
    {
        private readonly IMemoryCache _memoryCache;

        public CachedFileRepository(IUnitOfWorkProvider provider, IMemoryCache memoryCache) 
            : base(provider)
        {
            _memoryCache = memoryCache;
        }

        public override async Task<IList<File>> GetByIds(IEnumerable<Guid> ids, params Expression<Func<File, object>>[] includes)
        {
            var missingIds = new List<Guid>();
            var result = new List<File>();

            // We doesnt cache includes with files
            if (includes.Length == 0)
            {
                foreach (var id in ids)
                {
                    if (_memoryCache.TryGetValue(id, out File file))
                    {
                        result.Add(file);
                        continue;;
                    }
                    missingIds.Add(id);
                }
            }
            else
            {
                missingIds.AddRange(ids);
            }

            var downloadedFiles = await  base.GetByIds(missingIds, includes);

            foreach (var file in downloadedFiles)
            {
                // ToDo save only file entity and not connected entities
                _memoryCache.Set(file.Id, file);
            }

            result.AddRange(downloadedFiles);
            return result;
        }
    }
}