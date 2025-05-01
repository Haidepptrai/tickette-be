using Microsoft.Extensions.Options;
using Polly;
using RedLockNet;
using StackExchange.Redis;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common.Exceptions;
using Tickette.Domain.Entities;

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
    private static readonly IAsyncPolicy _retryPolicy = Policy
        .Handle<RedisException>()
        .Or<TimeoutException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)), // Exponential backoff
            onRetry: (exception, timeSpan, retryCount, context) =>
            {
                // Optional: log each retry
            });

    /// <summary>
    /// Reserves tickets with seat information
    /// </summary>
    /// <param name="tickets">Reservation information</param>
    /// <param name="userId">User unique ID who make ticketReservationInfo</param>
    /// <returns>True if ticketReservationInfo was successful</returns>
    public async Task<bool> ReserveTicketsAsync(Guid userId, ICollection<TicketReservation> tickets)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);

            List<IRedLock>? seatLocks = null;

            try
            {
                // Only acquire seat locks if needed
                if (tickets.Any(t => t.SeatsChosen?.Any() == true))
                {
                    seatLocks = await PreSeatsReservationAsync(tickets, _lockManager);
                }

                foreach (var ticket in tickets)
                {
                    string inventoryKey = RedisKeys.GetTicketQuantityKey(ticket.Id);

                    if (ticket.SeatsChosen != null && ticket.SeatsChosen.Any())
                    {
                        var userReservationKey = RedisKeys.GetUserTicketReservationKey(ticket.Id, userId);
                        var reservedSeatsKey = RedisKeys.GetTicketReservedSeatsKey(ticket.Id);
                        var reservedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                        // 1. Build seat label list: "B20,C15"
                        var seatLabels = ticket.SeatsChosen
                            .Select(seat => $"{seat.RowName}:{seat.SeatNumber}")
                            .Distinct()
                            .ToList();

                        var reservedSeats = await db.SetMembersAsync(reservedSeatsKey);
                        var alreadyReservedSeats = seatLabels.Intersect(reservedSeats.Select(r => r.ToString())).ToList();

                        // Abort if any seat is already reserved
                        if (alreadyReservedSeats.Any())
                        {
                            throw new SeatOrderedException($"Seat {string.Join(", ", alreadyReservedSeats)} has been taken");
                        }

                        var seatListStr = string.Join(",", seatLabels);

                        // 2. Store user’s reservation metadata
                        var reservationMeta = new HashEntry[]
                        {
                            new("quantity", ticket.Quantity),
                            new("reserved_at", reservedAt),
                            new("seats", seatListStr)
                        };

                        await db.HashSetAsync(userReservationKey, reservationMeta);

                        await db.StringDecrementAsync(inventoryKey, seatLabels.Count());

                        // 3. Add each seat to the ticket’s reserved set
                        var redisSeats = seatLabels.Select(s => (RedisValue)s).ToArray();
                        await db.SetAddAsync(reservedSeatsKey, redisSeats);
                    }
                    else
                    {
                        var redisKeys = new List<RedisKey>();
                        var redisArgs = new List<RedisValue>
                        {
                            userId.ToString(),
                            DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        };

                        string reservationKey = RedisKeys.GetReservationKey(ticket.Id, userId);
                        redisKeys.Add(inventoryKey);
                        redisKeys.Add(reservationKey);
                        redisArgs.Add(ticket.Quantity);

                        var result = await db.ScriptEvaluateAsync(
                            RedisLuaScripts.ReserveTicket, // Batched Lua script
                            redisKeys.ToArray(),
                            redisArgs.ToArray());

                        if (result.Resp2Type == ResultType.Array)
                        {
                            var resultArray = (RedisResult[])result;
                            var errorCode = (int)(long)resultArray[0];
                            var errorIndex = (int)(long)resultArray[1];

                            switch (errorCode)
                            {
                                case -1:
                                    throw new Exception($"Inventory key missing for ticket at index {errorIndex}");
                                case -2:
                                    throw new TicketReservationException(
                                        $"Not enough inventory for ticket at index {errorIndex}");
                            }
                        }

                        if ((long)result != 1)
                        {
                            throw new Exception("Unknown Redis error while reserving tickets without seats.");
                        }

                    }
                }

                return true;
            }
            finally
            {
                // Always release seat locks if acquired
                if (seatLocks != null)
                {
                    foreach (var l in seatLocks)
                        l.Dispose();
                }
            }
        });
    }

    /// <summary>
    /// Validates if a ticketReservationInformation exists and is still valid
    /// </summary>
    public async Task<bool> ValidateReservationAsync(Guid userId, TicketReservation ticketReservationInformation)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {

            var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);
            var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            const long reservationTimeoutSeconds = 1 * 60;

            if (ticketReservationInformation.SeatsChosen != null)
            {
                foreach (var seat in ticketReservationInformation.SeatsChosen)
                {
                    // 1. Get user-specific reservation key
                    var reservationKey = RedisKeys.GetUserTicketReservationKey(ticketReservationInformation.Id, userId);

                    // 2. Fetch full metadata hash
                    var hash = await db.HashGetAllAsync(reservationKey);
                    if (hash.Length == 0)
                        return false;

                    var hashDict = hash.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

                    // 3. Basic field checks
                    if (!hashDict.TryGetValue("reserved_at", out var reservedAtRaw) ||
                        !long.TryParse(reservedAtRaw, out var reservedAt) ||
                        nowUnix - reservedAt >= reservationTimeoutSeconds)
                        return false;

                    if (!hashDict.TryGetValue("seats", out var seatStr) || string.IsNullOrWhiteSpace(seatStr))
                        return false;

                    var reservedSeats = seatStr.Split(',').Select(s => s.Trim().ToUpperInvariant()).ToHashSet();
                    var requestedSeats = ticketReservationInformation.SeatsChosen
                        .Select(s => $"{s.RowName}{s.SeatNumber}".ToUpperInvariant())
                        .ToHashSet();

                    // 4. Ensure requested seats are all in the reserved list
                    if (!requestedSeats.SetEquals(reservedSeats))
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
        });
    }

    /// <summary>
    /// Releases a reservation and its seats
    /// </summary>
    public async Task<bool> ReleaseReservationAsync(Guid userId, TicketReservation ticketReservationInfo)
    {
        var db = _redis.GetDatabase(_redisSettings.DefaultDatabase);
        var inventoryKey = RedisKeys.GetTicketQuantityKey(ticketReservationInfo.Id);

        if (ticketReservationInfo.SeatsChosen != null && ticketReservationInfo.SeatsChosen.Any())
        {
            var reservationKey = RedisKeys.GetUserTicketReservationKey(ticketReservationInfo.Id, userId);
            var reservedSeatsKey = RedisKeys.GetTicketReservedSeatsKey(ticketReservationInfo.Id);

            // 1. Get seats from reservation hash
            var seatField = await db.HashGetAsync(reservationKey, "seats");
            var reservedSeats = seatField.IsNullOrEmpty
                ? []
                : seatField.ToString().Split(',').Select(s => s.Trim()).ToArray();

            // 2. Remove seat labels from the global reserved set
            if (reservedSeats.Length > 0)
            {
                var redisSeats = reservedSeats.Select(s => (RedisValue)s).ToArray();
                await db.SetRemoveAsync(reservedSeatsKey, redisSeats);
            }

            // 3. Delete user reservation metadata
            await db.KeyDeleteAsync(reservationKey);

            var quantity = ticketReservationInfo.Quantity;
            // 4. Restore ticket quantity
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
                var bookedSeatKey = RedisKeys.GetBookedSeatKey(ticketReservationInfo.Id, seat.RowName, seat.SeatNumber);


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

    /// <summary>
    /// Attempts to acquire all seat locks across multiple tickets before processing reservation.
    /// </summary>
    /// <param name="tickets">List of ticket reservations with seat selections.</param>
    /// <param name="lockManager">LockManage parameters since static function cannot use directly.</param>
    /// <returns>List of acquired IRedLock instances. Caller is responsible for disposing them.</returns>
    /// <exception cref="InvalidOperationException">Thrown if any seat is already locked.</exception>
    public static async Task<List<IRedLock>?> PreSeatsReservationAsync(
        ICollection<TicketReservation> tickets,
        LockManager lockManager)
    {
        var acquiredLocks = new List<IRedLock>();

        // Flatten all seat+ticket combinations for sorting
        var seatTargets = new List<(Guid TicketId, SeatOrder Seat)>();

        foreach (var ticket in tickets)
        {
            if (ticket.SeatsChosen != null && ticket.SeatsChosen.Any())
            {
                foreach (var seat in ticket.SeatsChosen)
                {
                    seatTargets.Add((ticket.Id, seat));
                }
            }
            else
            {
                return null;
            }
        }

        // Sort by lock key to prevent deadlocks
        seatTargets = seatTargets
            .OrderBy(x => RedisKeys.GetLockedSeat(x.TicketId, x.Seat))
            .ToList();

        try
        {
            foreach (var (ticketId, seat) in seatTargets)
            {
                var locks = await lockManager.AcquireSeatLocksAsync(ticketId, new[] { seat });

                // Add only the acquired one (since it's one seat per call here)
                acquiredLocks.AddRange(locks);
            }

            return acquiredLocks;
        }
        catch
        {
            foreach (var l in acquiredLocks)
                l.Dispose();

            throw new SeatOrderedException("Seat already taken");
        }
    }

}
