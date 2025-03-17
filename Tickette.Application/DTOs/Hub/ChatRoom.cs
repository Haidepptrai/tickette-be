namespace Tickette.Application.DTOs.Hub;

public record ChatRoom
{
    public string RoomId { get; init; }

    public string CustomerConnectionString { get; init; }

    public string AgentConnectionString { get; init; }

    public ChatRoom(string roomId, string customerConnectionString, string agentConnectionString)
    {
        RoomId = roomId;
        CustomerConnectionString = customerConnectionString;
        AgentConnectionString = agentConnectionString;
    }
}