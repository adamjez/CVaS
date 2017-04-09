using System;
using CVaS.Shared.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace CVaS.UnitTests
{
    public class MemoryCacheTests
    {
        private readonly MemoryCache _cache;

        public MemoryCacheTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        [Fact]
        public void MemoryCache_SameKeyIntDifferentType_TwoEntriesExpected()
        {
            int id1 = 5;
            int id2 = id1;

            var obj1 = new object();
            var obj2 = new object();
            _cache.Set(id1, CacheType.AlgorithmRepository, obj1);
            _cache.Set(id2, CacheType.FileRepository, obj2);


            Assert.True(_cache.TryGetValue(id1, CacheType.AlgorithmRepository, out object result1));
            Assert.True(_cache.TryGetValue(id2, CacheType.FileRepository, out object result2));
            Assert.NotEqual(result1, result2);
            Assert.Equal(obj1, result1);
            Assert.Equal(obj2, result2);
        }

        [Fact]
        public void MemoryCache_SameKeySameType_OneEntryExpected()
        {
            int id1 = 5;
            int id2 = id1;

            var obj1 = new object();
            var obj2 = new object();
            _cache.Set(id1, CacheType.AlgorithmRepository, obj1);
            _cache.Set(id2, CacheType.AlgorithmRepository, obj2);


            Assert.True(_cache.TryGetValue(id1, CacheType.AlgorithmRepository, out object result));
            Assert.NotEqual(obj1, result);
            Assert.Equal(obj2, result);
        }

        [Fact]
        public void MemoryCache_SameGuidKeyDifferentType_TwoEntriesExpected()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = id1;

            var obj1 = new object();
            var obj2 = new object();
            _cache.Set(id1, CacheType.AlgorithmRepository, obj1);
            _cache.Set(id2, CacheType.FileRepository, obj2);


            Assert.True(_cache.TryGetValue(id1, CacheType.AlgorithmRepository, out object result1));
            Assert.True(_cache.TryGetValue(id2, CacheType.FileRepository, out object result2));
            Assert.NotEqual(result1, result2);
            Assert.Equal(obj1, result1);
            Assert.Equal(obj2, result2);
        }

        [Fact]
        public void MemoryCache_SameStringKeyDifferentType_TwoEntriesExpected()
        {
            string id1 = "sample string key";
            string id2 = id1;

            var obj1 = new object();
            var obj2 = new object();
            _cache.Set(id1, CacheType.AlgorithmRepository, obj1);
            _cache.Set(id2, CacheType.FileRepository, obj2);


            Assert.True(_cache.TryGetValue(id1, CacheType.AlgorithmRepository, out object result1));
            Assert.True(_cache.TryGetValue(id2, CacheType.FileRepository, out object result2));
            Assert.NotEqual(result1, result2);
            Assert.Equal(obj1, result1);
            Assert.Equal(obj2, result2);
        }

        [Fact]
        public void MemoryCache_SameGuidKeyOneType_TwoEntriesExpected()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = id1;

            var obj1 = new object();
            var obj2 = new object();
            _cache.Set(id1, CacheType.AlgorithmRepository, obj1);
            _cache.Set(id2, obj2);


            Assert.True(_cache.TryGetValue(id1, CacheType.AlgorithmRepository, out object result1));
            Assert.True(_cache.TryGetValue(id2, out object result2));
            Assert.NotEqual(result1, result2);
            Assert.Equal(obj1, result1);
            Assert.Equal(obj2, result2);
        }
    }
}