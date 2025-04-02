using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.Interfaces;
using Tickette.Domain.Enums;

namespace Tickette.Infrastructure.Persistence.Redis;

public class ReservationDbSyncHandler
{
    private readonly IApplicationDbContext _dbContext;

    public ReservationDbSyncHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExpireReservationInDatabaseAsync(Guid userId, Guid ticketId)
    {
        var reservation = await _dbContext.Reservations
            .Where(r => r.UserId == userId)
            .Include(r => r.Items)
            .ThenInclude(i => i.SeatAssignments)
            .FirstOrDefaultAsync(r => r.Items.Any(i => i.TicketId == ticketId && r.Status == ReservationStatus.Temporary));

        if (reservation == null)
            return false;

        reservation.MarkExpired();

        // Restore ticket inventory
        foreach (var item in reservation.Items.Where(i => i.TicketId == ticketId))
        {
            // Assume you have a related event or ticket entity to update inventory
            var eventEntity = await _dbContext.Tickets.FindAsync(item.TicketId);
            if (eventEntity != null)
            {
                eventEntity.IncreaseTickets(item.Quantity);
            }
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }
}
