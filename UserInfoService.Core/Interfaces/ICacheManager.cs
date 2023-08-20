using Microsoft.Extensions.Caching.Memory;

namespace UserInfoService.Core.Interfaces
{
    public interface ICacheManager<T>
    {
        void RemoveFromCache(string key);
        T? GetFromCache(string key);
        void SetToCache(string key, T objectToBeCached, MemoryCacheEntryOptions cachingOption);
        MemoryCacheEntryOptions GenerateMemoryCacheEntryOptions(TimeSpan slidingExpiration, TimeSpan absoluteExpiration, CacheItemPriority priority = CacheItemPriority.Normal);
    }
}
