using Microsoft.Extensions.Caching.Memory;

namespace UserInfoService.Core.Interfaces
{
    public interface ICacheManager<T>
    {
        void Remove(string key);
        T? Get(string key);
        void Set(string key, T objectToBeCached, MemoryCacheEntryOptions cachingOption);
        MemoryCacheEntryOptions GenerateMemoryCacheEntryOptions(TimeSpan slidingExpiration, TimeSpan absoluteExpiration, CacheItemPriority priority = CacheItemPriority.Normal);
    }
}
