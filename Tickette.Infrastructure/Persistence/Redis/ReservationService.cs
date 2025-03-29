using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;
using Tickette.Infrastructure.Helpers;

namespace Tickette.Infrastructure.Persistence.Redis;

public class ReservationService : IReservationService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisSettings _redisSettings;
    private readonly LockManager _lockManager;

    public ReservationService(IConnectionMultiplexer redis, IOptions<RedisSettings> redisSettings, LockManager lockManager)
    {
        _redis = redis;
        _lockManager = lockManager;
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
        var transaction = db.CreateTransaction();

        if (ticketReservationInfo.SeatsChosen != null)
        {
            await _lockManager.AcquireSeatLocksAsync(ticketReservationInfo.Id, ticketReservationInfo.SeatsChosen);

            foreach (var seat in ticketReservationInfo.SeatsChosen)
            {
                var reservedSeatKey = RedisKeys.GetReservedSeatKey(ticketReservationInfo.Id, seat.RowName, seat.SeatNumber);

                if (await db.KeyExistsAsync(reservedSeatKey))
                {
                    throw new InvalidOperationException($"Seat {seat.RowName}{seat.SeatNumber} is already reserved");
                }

                await db.HashSetAsync(reservedSeatKey, [
                    new("userId", userId.ToString()),
                    new("reserved_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                ]);

                await db.KeyExpireAsync(reservedSeatKey, TimeSpan.FromMinutes(15));
            }
        }
        else
        {
            string inventoryKey = RedisKeys.GetTicketQuantityKey(ticketReservationInfo.Id);
            string reservationKey = RedisKeys.GetReservationKey(ticketReservationInfo.Id, userId);

            // Check if reservation already exists
            if (await db.KeyExistsAsync(reservationKey))
            {
                // Remove the existing reservation
                var success = await ReleaseReservationAsync(userId, ticketReservationInfo);

                if (!success)
                {
                    return false;
                }
            }

            long newInventory = await db.StringDecrementAsync(inventoryKey, ticketReservationInfo.Quantity);

            if (newInventory < 0)
            {
                return false;
            }

            // Write reservation info
            await db.HashSetAsync(reservationKey, [
                new("quantity", ticketReservationInfo.Quantity),
                new("user_id", userId.ToString()),
                new("reserved_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            ]);

            // Set expiration for reservation
            await db.KeyExpireAsync(reservationKey, TimeSpan.FromMinutes(15));
        }

        return await transaction.ExecuteAsync();

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
    public async Task<bool> ReleaseReservationAsync(Guid userId, TicketReservation ticketReservationInfo)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);
        var reservationKey = RedisKeys.GetReservationKey(ticketReservationInfo.Id, userId);
        var inventoryKey = RedisKeys.GetTicketQuantityKey(ticketReservationInfo.Id);

        if (ticketReservationInfo.SeatsChosen != null)
        {
            foreach (var seat in ticketReservationInfo.SeatsChosen)
            {
                var reservedSeatKey = RedisKeys.GetReservedSeatKey(ticketReservationInfo.Id, seat.RowName, seat.SeatNumber);
                await db.KeyDeleteAsync(reservedSeatKey);
            }
        }
        else
        {
            RedisValue quantityValue = await db.HashGetAsync(reservationKey, "quantity");

            if (quantityValue.IsNull)
            {
                return true;
            }

            // Add back the quantity to the inventory
            long quantity = (long)quantityValue;
            await db.StringIncrementAsync(inventoryKey, quantity);

            // Delete reservation
            await db.KeyDeleteAsync(reservationKey);
        }

        return true;
    }
}