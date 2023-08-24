using Microsoft.Extensions.Caching.Memory;
using UserInfoService.Core.Interfaces;

namespace UserInfoService.Core.Managers
{
    // ToDo : This implementation needs to be moved to the infrastructure layer
    public class CacheManager<T> : ICacheManager<T>
    {
        private readonly IMemoryCache _cache;

        public CacheManager(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public T? Get(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return value;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void Set(string key, T objectToBeCached, MemoryCacheEntryOptions cachingOption)
        {
            _cache.Set(key, objectToBeCached, cachingOption);
        }

        public MemoryCacheEntryOptions GenerateMemoryCacheEntryOptions(TimeSpan slidingExpiration, TimeSpan absoluteExpiration,
            CacheItemPriority priority)
        {
            return new MemoryCacheEntryOptions()
                  .SetSlidingExpiration(slidingExpiration)
                  .SetAbsoluteExpiration(absoluteExpiration)
                  .SetPriority(priority);
        }
    }

    public static class CacheKeys
    {
        public static readonly string USERINFO_LIST = "USERINFO_LIST";
    }
}
