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
        string inventoryKey = RedisKeys.GetTicketQuantityKey(ticketReservationInfo.Id);


        if (ticketReservationInfo.SeatsChosen != null)
        {
            await _lockManager.AcquireSeatLocksAsync(ticketReservationInfo.Id, ticketReservationInfo.SeatsChosen);

            foreach (var seat in ticketReservationInfo.SeatsChosen)
            {
                var bookedSeatKey = RedisKeys.GetBookedSeatKey(ticketReservationInfo.Id, seat.RowName, seat.SeatNumber);
                var reservedSeatKey = RedisKeys.GetReservedSeatKey(ticketReservationInfo.Id, seat.RowName, seat.SeatNumber);

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

                await db.KeyExpireAsync(reservedSeatKey, TimeSpan.FromMinutes(15));
            }

            // Decrease inventory
            await db.StringDecrementAsync(inventoryKey, ticketReservationInfo.Quantity);
        }
        else
        {
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
    /// Validates if a ticketReservationInformation exists and is still valid
    /// </summary>
    public async Task<bool> ValidateReservationAsync(Guid userId, TicketReservation ticketReservationInformation)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);

        if (ticketReservationInformation.SeatsChosen != null)
        {
            foreach (var seat in ticketReservationInformation.SeatsChosen)
            {
                var reservedSeatKey = RedisKeys.GetReservedSeatKey(ticketReservationInformation.Id, seat.RowName, seat.SeatNumber);

                // 1. Check if seat ticketReservationInformation key exists
                if (!await db.KeyExistsAsync(reservedSeatKey))
                    return false;

                // 2. Check if this seat is held by *this user*
                var storedUserId = await db.HashGetAsync(reservedSeatKey, "userId");
                if (storedUserId.IsNullOrEmpty || storedUserId.ToString() != userId.ToString())
                    return false;

                // 3. Optional: Check if TTL is still valid (seat not close to expiration)
                var ttl = await db.KeyTimeToLiveAsync(reservedSeatKey);
                if (ttl.HasValue && ttl.Value.TotalSeconds <= 5)
                    return false;
            }
        }
        else
        {
            var reservationKey = RedisKeys.GetReservationKey(ticketReservationInformation.Id, userId);

            // 1. Check key exists
            if (!await db.KeyExistsAsync(reservationKey))
                return false;

            // 2. Optional: check that it has the required fields
            var quantity = await db.HashGetAsync(reservationKey, "quantity");
            if (quantity.IsNull || long.Parse(quantity) <= 0)
                return false;

            // 3. Optional: TTL check
            var ttl = await db.KeyTimeToLiveAsync(reservationKey);
            if (ttl.HasValue && ttl.Value.TotalSeconds <= 5)
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

    public async Task<bool> FinalizeSeatReservationAsync(Guid userId, TicketReservation ticketReservationInfo)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);

        if (ticketReservationInfo.SeatsChosen != null)
        {
            foreach (var seat in ticketReservationInfo.SeatsChosen)
            {
                var reservedSeatKey = RedisKeys.GetReservedSeatKey(ticketReservationInfo.Id, seat.RowName, seat.SeatNumber);
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
