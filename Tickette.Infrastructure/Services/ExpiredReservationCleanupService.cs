using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Helpers;
using Tickette.Infrastructure.Persistence;

namespace Tickette.Infrastructure.Services;

/// <summary>
/// Background service that periodically checks for expired reservations and restores inventory
/// </summary>
public class ExpiredReservationCleanupService : BackgroundService
{
    private readonly ILogger<ExpiredReservationCleanupService> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisSettings _redisSettings;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);
    private readonly IServiceProvider _serviceProvider;


    public ExpiredReservationCleanupService(
        ILogger<ExpiredReservationCleanupService> logger,
        IOptions<RedisSettings> redisSettings, IConnectionMultiplexer redis, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _redis = redis;
        _serviceProvider = serviceProvider;
        _redisSettings = redisSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Expired Reservation Cleanup Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredReservationsAsync(stoppingToken);
                await ProcessExpiredSeatReservationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing expired reservations");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Expired Reservation Cleanup Service is stopping");
    }

    private async Task ProcessExpiredReservationsAsync(CancellationToken stoppingToken)
    {
        // Use a scoped service to get the Redis connection
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);

        // Use server instance to scan for keys
        var server = _redis.GetServer(_redis.GetEndPoints()[0]);

        // Scan for reservation keys that might be expired but still exist in Redis
        var reservationKeyPattern = "Tickette:reservation:*";
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Track how many reservations were processed
        int processedCount = 0;

        // Use SCAN to find all reservation keys
        foreach (var key in server.Keys(pattern: reservationKeyPattern))
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            string keyStr = key.ToString();

            // Check if the key exists (it might have been deleted between the SCAN and now)
            if (!await db.KeyExistsAsync(keyStr))
                continue;

            // Check if this reservation has expired
            var reservedAtValue = await db.HashGetAsync(keyStr, "reserved_at");
            var quantityValue = await db.HashGetAsync(keyStr, "quantity");

            if (reservedAtValue.IsNull || quantityValue.IsNull)
                continue;

            long reservedAt = (long)reservedAtValue;
            long quantity = (long)quantityValue;

            // Check if reservation has expired (15 minutes = 900 seconds)
            if (now - reservedAt > 60)
            {
                // Parse the ticket ID from the key
                // Format is "Tickette:reservation:{ticketId}:{userId}"
                var parts = keyStr.Split(':');
                if (parts.Length >= 4)
                {
                    string ticketIdStr = parts[2];
                    string userId = parts[3];

                    if (Guid.TryParse(ticketIdStr, out Guid ticketId))
                    {
                        string inventoryKey = RedisKeys.GetTicketQuantityKey(ticketId);

                        // Restore the inventory atomically
                        // Only increment if the key still exists (we check inside the transaction)
                        var transaction = db.CreateTransaction();
                        transaction.AddCondition(Condition.KeyExists(keyStr));

                        // Add the quantity back to inventory
                        transaction.StringIncrementAsync(inventoryKey, quantity);

                        // Delete the reservation
                        transaction.KeyDeleteAsync(keyStr);

                        bool success = await transaction.ExecuteAsync();

                        if (success)
                        {
                            // Sync the reservation state in the database
                            var scope = _serviceProvider.CreateScope();
                            var reservationDbSync = scope.ServiceProvider.GetRequiredService<IReservationDbSyncService>();

                            await reservationDbSync.ReleaseReservationFromDatabaseAsync(Guid.Parse(userId), ticketId, true);

                            processedCount++;
                            _logger.LogInformation(
                                "Cleaned up expired reservation for ticket {TicketId}, restored {Quantity} to inventory",
                                ticketId, quantity);
                        }
                    }
                }
            }
        }

        if (processedCount > 0)
        {
            _logger.LogInformation("Processed {Count} expired reservations", processedCount);
        }
    }

    private async Task ProcessExpiredSeatReservationsAsync(CancellationToken stoppingToken)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);
        var server = _redis.GetServer(_redis.GetEndPoints()[0]);

        // Scan for reserved seat keys that might be expired but still exist in Redis
        var reservedSeatKeyPattern = "Tickette:seat_reservation:*";
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        int processedCount = 0;

        // Use SCAN to find all reserved seat keys
        foreach (var key in server.Keys(pattern: reservedSeatKeyPattern))
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            string keyStr = key.ToString();

            // Check if the key exists (it might have been deleted between the SCAN and now)
            if (!await db.KeyExistsAsync(keyStr))
                continue;

            // Check if this seat reservation has expired
            var reservedAtValue = await db.HashGetAsync(keyStr, "reserved_at");

            if (reservedAtValue.IsNull)
                continue;

            long reservedAt = (long)reservedAtValue;

            // Check if seat reservation has expired (15 minutes = 900 seconds)
            if (now - reservedAt > 60)
            {
                // Format is "Tickette:seat_reservation:{ticketId}:{userId}:seat:{rowName}:{seatNumber}"
                var parts = keyStr.Split(':');
                if (parts.Length >= 7)
                {
                    string ticketIdStr = parts[2];
                    string userId = parts[3];
                    string rowName = parts[5];
                    string seatNumber = parts[6];

                    if (Guid.TryParse(ticketIdStr, out Guid ticketId))
                    {
                        // Delete the seat reservation
                        bool success = await db.KeyDeleteAsync(keyStr);

                        if (success)
                        {

                            // Sync the seat reservation state in the database
                            var scope = _serviceProvider.CreateScope();
                            var reservationDbSync = scope.ServiceProvider.GetRequiredService<IReservationDbSyncService>();
                            await reservationDbSync.ReleaseReservationFromDatabaseAsync(Guid.Parse(userId), ticketId, true);

                            processedCount++;
                            _logger.LogInformation(
                                "Cleaned up expired seat reservation for ticket {TicketId}, seat {RowName}{SeatNumber}",
                                ticketId, rowName, seatNumber);
                        }
                    }
                }
            }
        }

        if (processedCount > 0)
        {
            _logger.LogInformation("Processed {Count} expired seat reservations", processedCount);
        }
    }
}