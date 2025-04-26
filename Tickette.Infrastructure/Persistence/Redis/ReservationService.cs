using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common.Exceptions;

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
        try
        {
            var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);
            var transaction = db.CreateTransaction();
            string inventoryKey = RedisKeys.GetTicketQuantityKey(ticketReservationInfo.Id);

            if (ticketReservationInfo.SeatsChosen != null)
            {
                await _lockManager.AcquireSeatLocksAsync(ticketReservationInfo.Id, ticketReservationInfo.SeatsChosen);

                var redisKeys = new List<RedisKey>
                {
                    RedisKeys.GetTicketQuantityKey(ticketReservationInfo.Id),
                    RedisKeys.GetSeatsReservationKey(ticketReservationInfo.Id, userId)
                };

                var redisArgs = new List<RedisValue>
                {
                    ticketReservationInfo.Quantity,
                    userId.ToString(),
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    ticketReservationInfo.SeatsChosen.Count * 2 // total seat validations (booked + reserved)
                };

                // Add booked + reserved seat keys for validation
                foreach (var seat in ticketReservationInfo.SeatsChosen)
                {
                    var bookedKey = RedisKeys.GetBookedSeatKey(ticketReservationInfo.Id, seat.RowName, seat.SeatNumber);
                    var reservedKey = RedisKeys.GetReservedSeatKey(ticketReservationInfo.Id, userId, seat.RowName,
                        seat.SeatNumber);

                    redisArgs.Add(bookedKey);
                    redisArgs.Add(reservedKey);
                }

                // Add seat reservation write commands (HSET for each reserved seat key)
                foreach (var seat in ticketReservationInfo.SeatsChosen)
                {
                    var reservedSeatKey = RedisKeys.GetReservedSeatKey(ticketReservationInfo.Id, userId, seat.RowName,
                        seat.SeatNumber);
                    redisArgs.Add(reservedSeatKey);
                    redisArgs.Add("userId");
                    redisArgs.Add(userId.ToString());

                    redisArgs.Add(reservedSeatKey);
                    redisArgs.Add("reserved_at");
                    redisArgs.Add(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                }

                var result = await db.ScriptEvaluateAsync(
                    RedisLuaScripts.ReserveTicketWithSeats,
                    redisKeys.ToArray(),
                    redisArgs.ToArray());

                if (result.Resp3Type == ResultType.Array)
                {
                    var conflictSeatKeys = ((RedisResult[])result)
                        .Select(x => (string)x)
                        .ToList();

                    var readableSeatLabels = conflictSeatKeys
                        .Select(k =>
                        {
                            var parts = k.Split(':');
                            var row = parts[^2]; // second-to-last part → "C"
                            var number = parts[^1]; // last part → "24"
                            return $"{row}{number}"; // → "C24"
                        })
                        .Distinct()
                        .OrderBy(x => x) // alphabetically, or you can custom sort
                        .ToList();
                    ;

                    var friendlyMessage = string.Join(", ", readableSeatLabels);

                    throw new SeatOrderedException($"Some seats are already reserved: {friendlyMessage}");

                }

                if ((long)result == -1)
                    throw new Exception("Inventory key not found.");
                if ((long)result == -2)
                    throw new SeatOrderedException("Not enough inventory.");

            }
            else
            {
                string reservationKey = RedisKeys.GetReservationKey(ticketReservationInfo.Id, userId);

                var luaScript = RedisLuaScripts.ReserveTicket;

                var result = (long)await db.ScriptEvaluateAsync(luaScript,
                    keys: new RedisKey[]
                    {
                        inventoryKey,
                        reservationKey
                    },
                    values: new RedisValue[]
                    {
                        ticketReservationInfo.Quantity,
                        userId.ToString(),
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    });

                if (result == -1)
                    throw new Exception("Inventory key not found");
                if (result == -2)
                    throw new TicketReservationException("No ticket left to buy-in");
            }


            return await transaction.ExecuteAsync();
        }
        catch (RedisConnectionException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates if a ticketReservationInformation exists and is still valid
    /// </summary>
    public async Task<bool> ValidateReservationAsync(Guid userId, TicketReservation ticketReservationInformation)
    {
        try
        {
            var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);
            var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            const long reservationTimeoutSeconds = 1 * 60;

            if (ticketReservationInformation.SeatsChosen != null)
            {
                foreach (var seat in ticketReservationInformation.SeatsChosen)
                {
                    var reservedSeatKey = RedisKeys.GetReservedSeatKey(ticketReservationInformation.Id, userId,
                        seat.RowName, seat.SeatNumber);

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
        catch (RedisConnectionException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Releases a reservation and its seats
    /// </summary>
    public async Task<bool> ReleaseReservationAsync(Guid userId, TicketReservation ticketReservationInfo)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);
        var inventoryKey = RedisKeys.GetTicketQuantityKey(ticketReservationInfo.Id);

        if (ticketReservationInfo.SeatsChosen != null)
        {
            foreach (var seat in ticketReservationInfo.SeatsChosen)
            {
                var reservationKey = RedisKeys.GetSeatsReservationKey(ticketReservationInfo.Id, userId);
                var reservedSeatKey = RedisKeys.GetReservedSeatKey(ticketReservationInfo.Id, userId, seat.RowName, seat.SeatNumber);
                await db.KeyDeleteAsync(reservedSeatKey);
                await db.KeyDeleteAsync(reservationKey);
            }

            // Add back the quantity to the inventory
            long quantity = ticketReservationInfo.Quantity;
            await db.StringIncrementAsync(inventoryKey, quantity);
        }
        else
        {
            var reservationKey = RedisKeys.GetReservationKey(ticketReservationInfo.Id, userId);
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
