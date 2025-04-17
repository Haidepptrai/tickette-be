using RedLockNet;
using Tickette.Application.Helpers;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Persistence.Redis;

public sealed class LockManager
{
    private readonly IDistributedLockFactory _lockFactory;

    public LockManager(IDistributedLockFactory lockFactory)
    {
        _lockFactory = lockFactory;
    }

    public async Task<List<IRedLock>> AcquireSeatLocksAsync(
        Guid ticketId,
        IEnumerable<SeatOrder> seats
        )
    {
        var acquiredLocks = new List<IRedLock>();

        try
        {
            foreach (var seat in seats)
            {
                var lockKey = RedisKeys.GetLockSeat(ticketId, seat);
                var seatLock = await _lockFactory.CreateLockAsync(lockKey, TimeSpan.FromSeconds(5));

                if (!seatLock.IsAcquired)
                {
                    throw new InvalidOperationException($"Failed to lock seat: {seat}");
                }

                acquiredLocks.Add(seatLock);
            }

            return acquiredLocks;
        }
        finally
        {
            foreach (var seatLock in acquiredLocks)
            {
                seatLock.Dispose();
            }
        }
    }
}