using Tickette.Application.DTOs.Hub;

namespace Tickette.Application.Common.Interfaces.Redis;

public interface IChatRoomManagementService
{
    Task CreateChatRoomAsync(ChatRoom roomInfo);
    Task<string[]> DeleteChatRoomAsync(string userConnectionString);
}