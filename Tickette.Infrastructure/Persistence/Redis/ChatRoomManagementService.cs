using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Tickette.Infrastructure.Persistence.Redis;

public class ChatRoomManagementService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisSettings _redisSettings;

    public ChatRoomManagementService(IConnectionMultiplexer redis, IOptions<RedisSettings> redisSettings)
    {
        _redis = redis;
        _redisSettings = redisSettings.Value;
    }

    private string GetChatRoomKey(string roomId) => $"{_redisSettings.InstanceName}chatroom:{roomId}";

    // Create chat room
    public async Task CreateChatRoomAsync(ChatRoom roomInfo)
    {
        try
        {
            var db = _redis.GetDatabase();
            var chatRoomKey = GetChatRoomKey(roomInfo.RoomId);

            await db.HashSetAsync(chatRoomKey, [
                new HashEntry("room_id", roomInfo.RoomId),
                new HashEntry("customer_connection_string", roomInfo.CustomerConnectionString),
                new HashEntry("agent_connection_string", roomInfo.AgentConnectionString)
            ]);

            // Store room in the connection's lookup
            await db.SetAddAsync($"connection_string:{roomInfo.CustomerConnectionString}", roomInfo.RoomId);
            await db.SetAddAsync($"connection_string:{roomInfo.AgentConnectionString}", roomInfo.RoomId);


            // Set expiration on chat room details to avoid memory leak
            await db.KeyExpireAsync(chatRoomKey, TimeSpan.FromMinutes(30));
        }
        catch (Exception ex)
        {
            // Log the exception
        }
    }

    public async Task<string[]> DeleteChatRoomAsync(string userConnectionString)
    {
        try
        {
            var db = _redis.GetDatabase();
            var roomIds = await db.SetMembersAsync($"connection_string:{userConnectionString}");
            foreach (var roomId in roomIds)
            {
                if (roomId.IsNullOrEmpty)
                {
                    continue;
                }

                var chatRoomKey = GetChatRoomKey(roomId);
                await db.KeyDeleteAsync(chatRoomKey);
            }

            await db.KeyDeleteAsync($"connection_string:{userConnectionString}");
            return roomIds.Select(x => x.ToString()).ToArray();
        }
        catch (Exception ex)
        {
            // Log the exception
            return [];
        }

    }
}

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