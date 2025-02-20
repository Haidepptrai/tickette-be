namespace Tickette.Application.Common.Interfaces.Redis;

public interface IRedisService
{
    Task<bool> SetAsync(string key, string value, int expirationMinutes);
    Task<string?> GetAsync(string key);
    Task<bool> KeyExistsAsync(string key);
    Task<bool> DeleteKeyAsync(string key);
    Task<long> IncrementAsync(string key, long value);
    Task<long> DecrementAsync(string key, long value);

    // Handle bulk/batch operations
    Task SetBatchAsync(Dictionary<string, string> keyValuePairs);
}
