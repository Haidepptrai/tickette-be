using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tickette.Infrastructure.Persistence;
using Tickette.Infrastructure.Persistence.Redis;

namespace Tickette.Infrastructure.Services;

public class ReservationSyncService : BackgroundService
{
    private readonly ILogger<ReservationSyncService> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceProvider _serviceProvider;
    private readonly RedisSettings _redisSettings;

    public ReservationSyncService(
        ILogger<ReservationSyncService> logger,
        IConnectionMultiplexer redis,
        IServiceProvider serviceProvider,
        IOptions<RedisSettings> redisSettings)
    {
        _logger = logger;
        _redis = redis;
        _serviceProvider = serviceProvider;
        _redisSettings = redisSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncReservationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Sync] Reservation sync failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task SyncReservationsAsync()
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);
        var server = _redis.GetServer(_redis.GetEndPoints()[0]);

        var keys = server.Keys(pattern: "reservation:*");

        foreach (var key in keys)
        {
            try
            {
                var keyStr = key.ToString();
                var hash = await db.HashGetAllAsync(keyStr);
                if (hash.Length == 0) continue;

                var reservedAtRaw = hash.FirstOrDefault(x => x.Name == "reserved_at").Value;
                var userIdRaw = hash.FirstOrDefault(x => x.Name == "user_id" || x.Name == "userId").Value;

                if (string.IsNullOrWhiteSpace(reservedAtRaw) || string.IsNullOrWhiteSpace(userIdRaw))
                    continue;

                var reservedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(reservedAtRaw));
                if (reservedAt.AddMinutes(15) > DateTimeOffset.UtcNow)
                    continue;

                var (ticketId, userId) = ParseTicketIdAndUserIdFromKey(keyStr);

                var _dbHandler = _serviceProvider.GetRequiredService<ReservationDbSyncHandler>();
                var result = await _dbHandler.ExpireReservationInDatabaseAsync(userId, ticketId);
                if (result)
                {
                    await db.KeyDeleteAsync(keyStr);
                    _logger.LogInformation($"[Sync] Expired reservation cleaned: Ticket {ticketId} / User {userId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Sync] Failed to process reservation key: {key}");
            }
        }
    }

    private (Guid ticketId, Guid userId) ParseTicketIdAndUserIdFromKey(string key)
    {
        var parts = key.Split(':');
        if (parts.Length != 3)
            throw new FormatException("Invalid Redis reservation key format.");

        return (Guid.Parse(parts[1]), Guid.Parse(parts[2]));
    }
}
