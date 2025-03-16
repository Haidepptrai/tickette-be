using Tickette.Application.DTOs.Hub;

namespace Tickette.Application.Common.Interfaces.Redis;

public interface IAgentAvailabilityService
{
    Task SetAgentAvailableAsync(string agentId, string agentConnectionString, int currentTotalUserHandle = 0);
    Task<AgentInformation?> GetNextAvailableAgentAsync();
    Task SetAgentUnavailableAsync(string agentId);
    Task RemoveAgentFromPool(string agentId);
}