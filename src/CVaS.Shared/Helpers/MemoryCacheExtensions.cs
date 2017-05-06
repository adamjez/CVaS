using System;
using Microsoft.Extensions.Caching.Memory;

namespace CVaS.Shared.Helpers
{
    public static class MemoryCacheExtensions
    {
        public static TItem Set<TItem>(this IMemoryCache cache, object key, CacheType type, TItem value, MemoryCacheEntryOptions options = null)
        {
            if(options != null)
                return cache.Set(new CacheKey(key, type), value, options);
            else
                return cache.Set(new CacheKey(key, type), value);
        }

        public static bool TryGetValue<TItem>(this IMemoryCache cache, object key, CacheType type, out TItem value)
        {
            return cache.TryGetValue(new CacheKey(key, type), out value);
        }

        public static void Remove(this IMemoryCache cache, object key, CacheType type)
        {
            cache.Remove(new CacheKey(key, type));
        }
    }

    public enum CacheType
    {
        FileRepository,
        AlgorithmRepository,
        ApiKey
        
    }

    internal struct CacheKey
    {
        public CacheKey(object key, CacheType type)
        {
            Key = key;
            Type = type;
        }

        public object Key;
        public CacheType Type;
    }
}