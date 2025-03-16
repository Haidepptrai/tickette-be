using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.DTOs.Hub;

namespace Tickette.Infrastructure.Persistence.Redis;

public class AgentAvailabilityService : IAgentAvailabilityService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisSettings _redisSettings;

    private const string AVAILABILITY_KEY = "agent:available";

    public AgentAvailabilityService(IConnectionMultiplexer redis, IOptions<RedisSettings> settings)
    {
        _redis = redis;
        _redisSettings = settings.Value;
    }

    private string GetAgentKey(string agentId) => $"{_redisSettings.InstanceName}agent:{agentId}";
    private string GetAvailableAgentsKey() => $"{_redisSettings.InstanceName}{AVAILABILITY_KEY}";

    public async Task SetAgentAvailableAsync(string agentId, string agentConnectionString, int currentTotalUserHandle = 0)
    {
        try
        {
            var db = _redis.GetDatabase();
            var agentKey = GetAgentKey(agentId);
            var availableAgentKey = GetAvailableAgentsKey();

            await db.HashSetAsync(agentKey, [
                new HashEntry("status", "available"),
                new HashEntry("connection_string", agentConnectionString),
                new HashEntry("current_total_user_handle", currentTotalUserHandle),
                new HashEntry("last_updated", DateTime.UtcNow.Ticks)
            ]);

            // Set expiration on agent details
            await db.KeyExpireAsync(agentKey, TimeSpan.FromMinutes(30));

            // Add to available agents sorted set with score based on current chats
            await db.SortedSetAddAsync(availableAgentKey, agentId, currentTotalUserHandle);
        }
        catch (Exception ex)
        {
            // Log the exception
        }
    }

    public async Task<AgentInformation?> GetNextAvailableAgentAsync()
    {
        try
        {
            var db = _redis.GetDatabase();
            var availableAgentKey = GetAvailableAgentsKey();

            // Get agent with lowest score (fewest chats)
            var agentEntry = await db.SortedSetRangeByScoreWithScoresAsync(availableAgentKey, take: 1);

            if (agentEntry.Length == 0) return null;

            var agentId = agentEntry[0].Element.ToString();

            // Increment the agent's score
            await db.SortedSetIncrementAsync(availableAgentKey, agentId, 1);

            // Update agent hash
            var agentKey = GetAgentKey(agentId);
            await db.HashIncrementAsync(agentKey, "current_total_user_handle");

            var agentConnectionString = await db.HashGetAsync(agentKey, "connection_string");

            if (agentConnectionString.IsNullOrEmpty) return null;

            var returnAgent = new AgentInformation(agentId, agentConnectionString);

            return returnAgent;
        }
        catch (Exception ex)
        {
            // Log the exception
            return null;
        }
    }

    public async Task SetAgentUnavailableAsync(string agentId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var agentKey = GetAgentKey(agentId);
            var availableAgentsKey = GetAvailableAgentsKey();

            // Remove from available agents
            await db.SortedSetRemoveAsync(availableAgentsKey, agentId);

            // Update agent status
            await db.HashSetAsync(agentKey, "status", "unavailable");
        }
        catch (Exception ex)
        {
        }
    }

    public async Task RemoveAgentFromPool(string agentId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var agentKey = GetAgentKey(agentId);

            await db.KeyDeleteAsync(agentKey);

            await db.SortedSetRemoveAsync(GetAvailableAgentsKey(), agentId);
        }
        catch (Exception ex)
        {
            // Log the exception
        }
    }

}