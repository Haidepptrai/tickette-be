using MailKit.Security;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Tickette.Infrastructure.Hubs;

public class ChatSupportHub : Hub
{
    private static readonly List<string> AgentPool = new();

    // Send message to a specific group
    public async Task SendMessage(string groupId, string message)
    {
        var userName = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Name);

        await Clients.Group(groupId).SendAsync("ReceiveMessage", userName, message);
    }

    public override async Task OnConnectedAsync()
    {
        var userRole = Context.User?.FindFirstValue(ClaimTypes.Role);

        if (userRole == "Agent")
        {
            await AgentAddInPool(Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? throw new AuthenticationException("Cannot find credential"));
        }
        else
        {
            await UserConnectToAgent();
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// When an agent connects, add them to the pool automatically
    /// </summary>
    private async Task AgentAddInPool(string agentId)
    {
        if (!AgentPool.Contains(agentId ?? throw new AuthenticationException("Cannot find credential")))
        {
            AgentPool.Add(Context.ConnectionId);
        }

        await Clients.All.SendAsync("AgentAdded", agentId);
    }

    /// <summary>
    /// When a user connects, try to connect them to an agent
    /// </summary>
    private async Task UserConnectToAgent()
    {
        // Create a group for the agent and user to communicate
        string groupId = $"{AgentPool[0]}_{Context.ConnectionId}";

        // Add both the agent and user to the group
        await Groups.AddToGroupAsync(AgentPool[0], groupId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);

        await Clients.Group(groupId).SendAsync("SupportRoomCreated", groupId);
    }
}