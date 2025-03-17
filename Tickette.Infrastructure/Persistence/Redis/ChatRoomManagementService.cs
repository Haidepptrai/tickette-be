using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.DTOs.Hub;

namespace Tickette.Infrastructure.Persistence.Redis;

public class ChatRoomManagementService : IChatRoomManagementService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisSettings _redisSettings;

    public ChatRoomManagementService(IConnectionMultiplexer redis, IOptions<RedisSettings> redisSettings)
    {
        _redis = redis;
        _redisSettings = redisSettings.Value;
    }

    private string GetUserConnectionString(string userConnectionString) => $"{_redisSettings.InstanceName}connection_string:{userConnectionString}";

    /// <summary>
    /// When a chat room is created, store the room information in the connection's lookup
    /// </summary>
    /// <param name="roomInfo">
    /// Include information about room id, connection string of
    /// both customer and agent
    /// </param>
    /// <returns></returns>
    public async Task CreateChatRoomAsync(ChatRoom roomInfo)
    {
        try
        {
            var db = _redis.GetDatabase();

            // Store room in the connection's lookup
            await db.SetAddAsync(GetUserConnectionString(roomInfo.CustomerConnectionString), roomInfo.RoomId);
            await db.SetAddAsync(GetUserConnectionString(roomInfo.AgentConnectionString), roomInfo.RoomId);
        }
        catch (Exception ex)
        {
            // Log the exception
        }
    }

    /// <summary>
    /// When a chat room is deleted, remove the room information from the connection's lookup
    /// </summary>
    /// <param name="userConnectionString">
    /// Connection string from either customer or agent that disconnected
    /// </param>
    /// <returns>A list of room that the user left</returns>

    public async Task<string[]> DeleteChatRoomAsync(string userConnectionString)
    {
        try
        {
            var db = _redis.GetDatabase();
            var roomIds = await db.SetMembersAsync(GetUserConnectionString(userConnectionString));

            await db.KeyDeleteAsync(GetUserConnectionString(userConnectionString));
            return roomIds.Select(x => x.ToString()).ToArray();
        }
        catch (Exception ex)
        {
            // Log the exception
            return [];
        }

    }
}