using Microsoft.Extensions.Caching.Memory;
using UserInfoService.Core.Interfaces;

namespace UserInfoService.Core.Managers
{
    public class CacheManager<T> : ICacheManager<T>
    {
        private readonly IMemoryCache _cache;

        public CacheManager(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public T? GetFromCache(string key)
        {
           _cache.TryGetValue(key, out T? value);
            return value;
        }

        public void RemoveFromCache(string key)
        {
            _cache.Remove(key);
        }

        public void SetToCache(string key, T objectToBeCached, MemoryCacheEntryOptions cachingOption)
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
