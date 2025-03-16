using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Authentication;
using System.Security.Claims;
using Tickette.Application.Common.Interfaces.Redis;

namespace Tickette.Infrastructure.Hubs;

public class ChatSupportHub : Hub
{
    private readonly IAgentAvailabilityService _agentManagementService;

    public ChatSupportHub(IAgentAvailabilityService agentManagementService)
    {
        _agentManagementService = agentManagementService;
    }

    // Send message to a specific group
    public async Task SendMessage(string supportRoomId, string message, string userName, string userEmail)
    {
        var userId = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? throw new AuthenticationException("Cannot find credential");

        await Clients.Group(supportRoomId).SendAsync("ReceiveMessage", userId, message);
        await Clients.Group(supportRoomId).SendAsync("UserInformationToAgent", userName, userEmail);
    }

    public override async Task OnConnectedAsync()
    {
        var userRole = Context.User?.FindFirstValue(ClaimTypes.Role);
        // Get the group that user is in

        if (userRole == "Agent")
        {
            var userId = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? throw new AuthenticationException("Cannot find credential");
            await AgentAddInPool(userId);
        }
        else
        {
            await UserConnectToAgent();
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userRole = Context.User?.FindFirstValue(ClaimTypes.Role);

        if (userRole == "Agent")
        {
            var userId = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? throw new AuthenticationException("Cannot find credential");
            await _agentManagementService.RemoveAgentFromPool(userId);
        }


        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// When an agent connects, add them to the pool automatically
    /// </summary>
    private async Task AgentAddInPool(string agentId)
    {
        await _agentManagementService.SetAgentAvailableAsync(agentId, Context.ConnectionId);
    }

    /// <summary>
    /// When a user connects, try to connect them to an agent
    /// </summary>
    private async Task UserConnectToAgent()
    {
        var getAvailableAgent = await _agentManagementService.GetNextAvailableAgentAsync();

        if (getAvailableAgent is null)
        {
            await Clients.Caller.SendAsync("NoAgentAvailable");
        }

        // Create a group for the agent and user to communicate
        string groupId = $"{getAvailableAgent?.ConnectionString}_{Context.ConnectionId}";

        // Add both the agent and user to the group
        await Groups.AddToGroupAsync(getAvailableAgent.ConnectionString, groupId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);

        await Clients.Group(groupId).SendAsync("SupportRoomCreated", groupId, getAvailableAgent.AgentId);
    }
}