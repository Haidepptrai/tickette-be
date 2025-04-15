using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common.Exceptions;
using Tickette.Infrastructure.Helpers;

namespace Tickette.Infrastructure.Persistence.Redis;

public class ReservationService : IReservationService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisSettings _redisSettings;
    private readonly LockManager _lockManager;
    private readonly ReservationDbSyncHandler _dbSyncHandler;

    public ReservationService(IConnectionMultiplexer redis, IOptions<RedisSettings> redisSettings, LockManager lockManager, ReservationDbSyncHandler dbSyncHandler)
    {
        _redis = redis;
        _lockManager = lockManager;
        _dbSyncHandler = dbSyncHandler;
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
        string inventoryKey = RedisKeys.GetTicketQuantityKey(ticketReservationInfo.Id);

        if (ticketReservationInfo.SeatsChosen != null)
        {
            await _lockManager.AcquireSeatLocksAsync(ticketReservationInfo.Id, ticketReservationInfo.SeatsChosen);

            foreach (var seat in ticketReservationInfo.SeatsChosen)
            {
                var bookedSeatKey = RedisKeys.GetBookedSeatKey(ticketReservationInfo.Id, seat.RowName, seat.SeatNumber);
                var reservedSeatKey = RedisKeys.GetReservedSeatKey(ticketReservationInfo.Id, userId, seat.RowName, seat.SeatNumber);

                // Check for permanent booking
                if (await db.KeyExistsAsync(bookedSeatKey))
                {
                    throw new SeatOrderedException($"Seat {seat.RowName}{seat.SeatNumber} is already booked.");
                }

                // Check for temporary reservation
                if (await db.KeyExistsAsync(reservedSeatKey))
                {
                    throw new SeatOrderedException($"Seat {seat.RowName}{seat.SeatNumber} is already reserved");
                }

                await db.HashSetAsync(reservedSeatKey, [
                    new("userId", userId.ToString()),
                    new("reserved_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                ]);
            }

            // Decrease inventory
            await db.StringDecrementAsync(inventoryKey, ticketReservationInfo.Quantity);
        }
        else
        {
            string reservationKey = RedisKeys.GetReservationKey(ticketReservationInfo.Id, userId);

            // Check if reservation already exists in Redis
            if (await db.KeyExistsAsync(reservationKey))
            {
                // Remove the reservation from Redis
                await ReleaseReservationAsync(userId, ticketReservationInfo);
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
        }

        return await transaction.ExecuteAsync();
    }

    /// <summary>
    /// Validates if a ticketReservationInformation exists and is still valid
    /// </summary>
    public async Task<bool> ValidateReservationAsync(Guid userId, TicketReservation ticketReservationInformation)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);
        var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        const long reservationTimeoutSeconds = 1 * 60;

        if (ticketReservationInformation.SeatsChosen != null)
        {
            foreach (var seat in ticketReservationInformation.SeatsChosen)
            {
                var reservedSeatKey = RedisKeys.GetReservedSeatKey(ticketReservationInformation.Id, userId, seat.RowName, seat.SeatNumber);

                // 1. Check if seat ticketReservationInformation key exists
                if (!await db.KeyExistsAsync(reservedSeatKey))
                    return false;

                // 2. Check if this seat is held by *this user*
                var storedUserId = await db.HashGetAsync(reservedSeatKey, "userId");
                if (storedUserId.IsNullOrEmpty || storedUserId.ToString() != userId.ToString())
                    return false;

                // 3. TTL check
                var reservedAtValue = await db.HashGetAsync(reservedSeatKey, "reserved_at");
                if (reservedAtValue.IsNull || !long.TryParse(reservedAtValue.ToString(), out var reservedAt))
                    return false;

                if (nowUnix - reservedAt >= reservationTimeoutSeconds)
                    return false;
            }
        }
        else
        {
            var reservationKey = RedisKeys.GetReservationKey(ticketReservationInformation.Id, userId);

            // 1. Check key exists
            if (!await db.KeyExistsAsync(reservationKey))
                return false;

            // 2. Check that it has the required fields
            var quantity = await db.HashGetAsync(reservationKey, "quantity");
            if (quantity.IsNull || long.Parse(quantity) <= 0)
                return false;

            // 3. TTL check
            var reservedAtValue = await db.HashGetAsync(reservationKey, "reserved_at");
            if (reservedAtValue.IsNull || !long.TryParse(reservedAtValue.ToString(), out var reservedAt))
                return false;

            if (nowUnix - reservedAt >= reservationTimeoutSeconds)
                return false;
        }

        return true;
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
                var reservedSeatKey = RedisKeys.GetReservedSeatKey(ticketReservationInfo.Id, userId, seat.RowName, seat.SeatNumber);
                await db.KeyDeleteAsync(reservedSeatKey);
            }

            // Add back the quantity to the inventory
            long quantity = ticketReservationInfo.Quantity;
            await db.StringIncrementAsync(inventoryKey, quantity);
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

    public async Task<bool> FinalizeSeatReservationAsync(Guid userId, TicketReservation ticketReservationInfo)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);

        if (ticketReservationInfo.SeatsChosen != null)
        {
            foreach (var seat in ticketReservationInfo.SeatsChosen)
            {
                var reservedSeatKey = RedisKeys.GetReservedSeatKey(ticketReservationInfo.Id, userId, seat.RowName, seat.SeatNumber);
                var bookedSeatKey = RedisKeys.GetBookedSeatKey(ticketReservationInfo.Id, seat.RowName, seat.SeatNumber);

                // Delete temporary reservation key (if it exists)
                await db.KeyDeleteAsync(reservedSeatKey);

                // Set permanent booked key
                await db.StringSetAsync(bookedSeatKey, userId.ToString());
            }
        }
        else
        {
            var reservationKey = RedisKeys.GetReservationKey(ticketReservationInfo.Id, userId);
            // Delete reservation
            await db.KeyDeleteAsync(reservationKey);
        }

        return true;
    }
}
