using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Tickette.Application.Common.Interfaces.Redis;

namespace Tickette.Infrastructure.Persistence.Redis
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _database;

        public RedisService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Redis") ?? throw new ArgumentException("Missing Redis Connection String");
            var redis = ConnectionMultiplexer.Connect(connectionString);
            _database = redis.GetDatabase();
        }

        public async Task<bool> SetAsync(string key, string value, int expirationMinutes)
        {
            if (expirationMinutes > 0)
            {
                return await _database.StringSetAsync(key, value, TimeSpan.FromMinutes(expirationMinutes));
            }

            return await _database.StringSetAsync(key, value); // No expiration
        }

        public async Task<string?> GetAsync(string key)
        {
            return await _database.StringGetAsync(key);
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task<bool> DeleteKeyAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        public async Task<long> IncrementAsync(string key, long value)
        {
            return await _database.StringIncrementAsync(key, value);
        }

        public async Task<long> DecrementAsync(string key, long value)
        {
            return await _database.StringDecrementAsync(key, value);
        }

        public async Task<bool> AcquireLockAsync(string key, TimeSpan expiry)
        {
            return await _database.LockTakeAsync(key, Environment.MachineName, expiry);
        }

        public async Task<bool> ReleaseLockAsync(string key)
        {
            return await _database.LockReleaseAsync(key, Environment.MachineName);
        }
    }
}