using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Entities;
using Tickette.Infrastructure.Helpers;

namespace Tickette.Infrastructure.Persistence.Redis;

public class ReservationService : IReservationService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisSettings _redisSettings;

    public ReservationService(IConnectionMultiplexer redis, IOptions<RedisSettings> redisSettings)
    {
        _redis = redis;
        _redisSettings = redisSettings.Value;
    }

    /// <summary>
    /// Reserves tickets with seat information
    /// </summary>
    /// <param name="ticketReservationInfo">Reservation information</param>
    /// <param name="userId">User unique ID who make ticketReservationInfo</param>
    /// <returns>True if ticketReservationInfo was successful</returns>
    public async Task<bool> ReserveTicketsAsync(Guid userId, TicketReservation ticketReservationInfo)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);
        var reservationKey = RedisKeys.GetReservationKey(ticketReservationInfo.Id, userId);
        var seatsKey = RedisKeys.GetSeatsKey(ticketReservationInfo.Id);
        var inventoryKey = RedisKeys.GetTicketQuantityKey(ticketReservationInfo.Id);

        var transaction = db.CreateTransaction();

        // Add condition: check if enough tickets are available
        // Get current inventory before transaction
        long currentInventory = await db.StringGetAsync(inventoryKey).ContinueWith(t =>
            t.Result.HasValue ? (long)t.Result : 0);

        if (currentInventory < ticketReservationInfo.Quantity)
        {
            return false; // Not enough tickets available
        }

        // Add condition to ensure inventory hasn't changed
        transaction.AddCondition(Condition.StringEqual(inventoryKey, currentInventory));

        // Prepare ticketReservationInfo data
        var reservationData = new HashEntry[]
        {
        new HashEntry("id", ticketReservationInfo.Id.ToString()),
        new HashEntry("quantity", ticketReservationInfo.Quantity),
        new HashEntry("sectionName", ticketReservationInfo.SectionName ?? string.Empty),
        new HashEntry("seats", JsonSerializer.Serialize(ticketReservationInfo.SeatsChosen))
        };

        List<Task> tasks = new List<Task>();

        // Check seat availability if specific seats were chosen
        if (ticketReservationInfo.SeatsChosen != null && ticketReservationInfo.SeatsChosen.Any())
        {
            var seatIds = ticketReservationInfo.SeatsChosen.Select(s => (RedisValue)$"{s.RowName}:{s.SeatNumber}").ToArray();

            // Add condition: seats must not already be taken
            transaction.AddCondition(Condition.SetNotContains(seatsKey, seatIds[0]));

            // Mark seats as reserved
            tasks.Add(transaction.SetAddAsync(seatsKey, seatIds));
        }

        // Store ticketReservationInfo data
        tasks.Add(transaction.HashSetAsync(reservationKey, reservationData));

        // Set expiration
        tasks.Add(transaction.KeyExpireAsync(reservationKey, TimeSpan.FromMinutes(15)));

        // Decrement ticket quantity atomically as part of the transaction
        tasks.Add(transaction.StringDecrementAsync(inventoryKey, ticketReservationInfo.Quantity));

        // Execute transaction
        bool success = await transaction.ExecuteAsync();

        if (success)
        {
            await Task.WhenAll(tasks);
        }

        return success;
    }

    /// <summary>
    /// Validates if a reservation exists and is still valid
    /// </summary>
    public async Task<bool> ValidateReservationAsync(Guid ticketId, Guid userId)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);
        var reservationKey = RedisKeys.GetReservationKey(ticketId, userId);

        return await db.KeyExistsAsync(reservationKey);
    }

    /// <summary>
    /// Releases a reservation and its seats
    /// </summary>
    public async Task<bool> ReleaseReservationAsync(Guid ticketId, Guid userId)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);
        var reservationKey = RedisKeys.GetReservationKey(ticketId, userId);
        var seatsKey = RedisKeys.GetSeatsKey(ticketId);
        var inventoryKey = RedisKeys.GetTicketQuantityKey(ticketId);

        // Get reservation to find seat information and quantity
        var reservation = await GetReservationAsync(ticketId, userId);
        if (reservation == null)
            return false;

        // Start a transaction for atomic operations
        var transaction = db.CreateTransaction();

        // Return ticket quantity back to inventory
        await transaction.StringIncrementAsync(inventoryKey, reservation.Quantity);

        if (reservation.SeatsChosen != null)
        {
            var seatIds = reservation.SeatsChosen.Select(s => (RedisValue)$"{s.RowName}:{s.SeatNumber}").ToArray();

            // Remove seats from the reserved set
            await transaction.SetRemoveAsync(seatsKey, seatIds);
        }

        // Delete the reservation
        await transaction.KeyDeleteAsync(reservationKey);

        // Execute all operations atomically
        return await transaction.ExecuteAsync();
    }

    /// <summary>
    /// Gets a reservation by ID
    /// </summary>
    private async Task<TicketReservation?> GetReservationAsync(Guid ticketId, Guid userId)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);
        var reservationKey = RedisKeys.GetReservationKey(ticketId, userId);

        var hashEntries = await db.HashGetAllAsync(reservationKey);

        if (hashEntries.Length == 0)
            return null;

        var values = hashEntries.ToDictionary(h => h.Name.ToString(), h => h.Value.ToString());

        IEnumerable<SeatOrder>? seats = null;
        if (values.TryGetValue("seats", out var seatsJson) && !string.IsNullOrEmpty(seatsJson))
        {
            seats = JsonSerializer.Deserialize<IEnumerable<SeatOrder>>(seatsJson);
        }

        return new TicketReservation(
            Guid.Parse(values["id"]),
            int.Parse(values["quantity"]),
            values["sectionName"],
            seats
        );
    }
}