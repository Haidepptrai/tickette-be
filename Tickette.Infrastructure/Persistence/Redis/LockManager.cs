using RedLockNet;
using Tickette.Application.Common.Constants;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Persistence.Redis;

public sealed class LockManager
{
    private readonly IDistributedLockFactory _lockFactory;

    public LockManager(IDistributedLockFactory lockFactory)
    {
        _lockFactory = lockFactory;
    }

    public async Task<List<IRedLock>> AcquireSeatLocksAsync(Guid ticketId, IEnumerable<SeatOrder> seats)
    {
        var acquiredLocks = new List<IRedLock>();

        foreach (var seat in seats)
        {
            var lockKey = RedisKeys.GetLockedSeat(ticketId, seat);
            var seatLock = await _lockFactory.CreateLockAsync(lockKey, TimeSpan.FromSeconds(5));

            if (!seatLock.IsAcquired)
            {
                // Clean up previously acquired locks before throwing
                foreach (var l in acquiredLocks)
                    l.Dispose();

                throw new InvalidOperationException($"Failed to lock seat: {seat}");
            }

            acquiredLocks.Add(seatLock);
        }

        return acquiredLocks; // caller must Dispose()
    }
}