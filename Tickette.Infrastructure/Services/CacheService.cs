using Microsoft.Extensions.Caching.Memory;
using Tickette.Application.Common.Interfaces;

namespace Tickette.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public void SetCacheValue<T>(string key, T value, TimeSpan? expiration = null)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(expiration ?? DefaultExpiration);

        _memoryCache.Set(key, value, cacheEntryOptions);
    }

    public T? GetCacheValue<T>(string key)
    {
        _memoryCache.TryGetValue(key, out T? value);
        return value;
    }

    public void RemoveCacheValue(string key)
    {
        _memoryCache.Remove(key);
    }
}