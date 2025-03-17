namespace Tickette.Application.DTOs.Hub;

public record AgentInformation
{
    public string AgentId { get; init; }

    public string ConnectionString { get; init; }

    public AgentInformation(string agentId, string connectionString)
    {
        AgentId = agentId;
        ConnectionString = connectionString;
    }
}