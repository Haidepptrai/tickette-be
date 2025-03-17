using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Authentication;
using System.Security.Claims;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.DTOs.Hub;

namespace Tickette.Infrastructure.Hubs;

public class ChatSupportHub : Hub
{
    private readonly IAgentAvailabilityService _agentManagementService;
    private readonly IChatRoomManagementService _chatRoomManagementService;

    public ChatSupportHub(IAgentAvailabilityService agentManagementService, IChatRoomManagementService chatRoomManagementService)
    {
        _agentManagementService = agentManagementService;
        _chatRoomManagementService = chatRoomManagementService;
    }

    // Send message to a specific group
    public async Task SendMessage(string supportRoomId, string message, string userName, string userEmail)
    {
        var userId = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? throw new AuthenticationException("Cannot find credential");

        await Clients.Group(supportRoomId).SendAsync("ReceiveMessage", supportRoomId, userId, message);
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

        var deletedRooms = await _chatRoomManagementService.DeleteChatRoomAsync(Context.ConnectionId);

        foreach (var room in deletedRooms)
        {
            await Clients.Group(room).SendAsync("UserDisconnected");
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
    public async Task UserConnectToAgent(string customerName, string customerEmail)
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

        var chatRoomInfo = new ChatRoom(groupId, Context.ConnectionId, getAvailableAgent.ConnectionString);

        await _chatRoomManagementService.CreateChatRoomAsync(chatRoomInfo);

        await Clients.Group(groupId).SendAsync("SupportRoomCreated", groupId, getAvailableAgent.AgentId);
        await Clients.Group(groupId).SendAsync("UserInformationToAgent", groupId, customerName, customerEmail);

    }
}