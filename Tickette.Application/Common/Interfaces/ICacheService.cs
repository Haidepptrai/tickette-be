namespace Tickette.Application.Common.Interfaces;

// For In-Memory Caching
public interface ICacheService
{
    void SetCacheValue<T>(string key, T value, TimeSpan? expiration = null);
    T? GetCacheValue<T>(string key);
    void RemoveCacheValue(string key);
}