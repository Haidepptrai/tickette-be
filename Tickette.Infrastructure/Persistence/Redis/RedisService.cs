using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tickette.Application.Common.Interfaces.Redis;

namespace Tickette.Infrastructure.Persistence.Redis;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisSettings _redisSettings;

    public RedisService(IOptions<RedisSettings> configuration, IConnectionMultiplexer redis)
    {
        _redis = redis;
        _redisSettings = configuration.Value;
    }

    public async Task<bool> SetAsync(string key, string value, int expirationMinutes)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);

        if (expirationMinutes > 0)
        {
            return await db.StringSetAsync(key, value, TimeSpan.FromMinutes(expirationMinutes));
        }

        return await db.StringSetAsync(key, value); // No expiration
    }

    public async Task<string?> GetAsync(string key)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);

        return await db.StringGetAsync(key);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);

        return await db.KeyExistsAsync(key);
    }

    public async Task<bool> DeleteKeyAsync(string key)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);

        return await db.KeyDeleteAsync(key);
    }

    public async Task<long> IncrementAsync(string key, long value)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);

        return await db.StringIncrementAsync(key, value);
    }

    public async Task<long> DecrementAsync(string key, long value)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);

        return await db.StringDecrementAsync(key, value);
    }

    public async Task SetBatchAsync(Dictionary<string, string> keyValuePairs)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);

        var batch = db.CreateBatch();
        var tasks = new List<Task>();

        foreach (var kvp in keyValuePairs)
        {
            tasks.Add(batch.StringSetAsync(kvp.Key, kvp.Value));
        }

        batch.Execute();
        await Task.WhenAll(tasks);
    }

}