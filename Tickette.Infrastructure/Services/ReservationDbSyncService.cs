using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Entities;
using Tickette.Domain.Enums;

namespace Tickette.Infrastructure.Services;

/// <summary>
/// This class is responsible for persisting reservations to the database.
/// Since Redis could potentially crash
/// We need to ensure that the reservation is saved in the database to recover it later.
/// </summary>
public class ReservationDbSyncService : IReservationDbSyncService
{
    private readonly IApplicationDbContext _dbContext;

    public ReservationDbSyncService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Persists a reserved ticket and its details to the database.
    /// </summary>
    /// <param name="userId">The ID of the user who made the reservation</param>
    /// <param name="ticketReservation">Ticket and seat details</param>
    /// <returns></returns>
    public async Task<bool> PersistReservationAsync(Guid userId, TicketReservation ticketReservation)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var existingReservation = await _dbContext.Reservations
                .Include(r => r.Items)
                .Where(r => r.UserId == userId && r.Status == ReservationStatus.Temporary && r.ExpiresAt <= DateTime.UtcNow)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (existingReservation != null)
            {
                var itemToRemove = existingReservation.Items
                    .FirstOrDefault(i => i.TicketId == ticketReservation.Id);

                if (itemToRemove != null)
                {
                    // Restore inventory
                    var ticketInventory = await _dbContext.Tickets.SingleOrDefaultAsync(t => t.Id == ticketReservation.Id);
                    if (ticketInventory != null)
                    {
                        ticketInventory.IncreaseTickets(itemToRemove.Quantity);
                    }

                    existingReservation.RemoveItem(itemToRemove);

                    // If reservation has no items left, delete it
                    if (!existingReservation.Items.Any())
                    {
                        _dbContext.Reservations.Remove(existingReservation);
                    }
                }
            }

            // Create a new reservation
            var newReservation = new Reservation(userId, DateTime.UtcNow.AddMinutes(1));

            newReservation.AddItem(
                ticketId: ticketReservation.Id,
                quantity: ticketReservation.Quantity,
                hasAssignedSeats: ticketReservation.SeatsChosen != null
            );

            if (ticketReservation.SeatsChosen != null)
            {
                var item = newReservation.Items.First();
                foreach (var seat in ticketReservation.SeatsChosen)
                {
                    item.AssignSeat(seat.RowName, seat.SeatNumber);
                }
            }

            // Decrease ticket inventory
            var tickets = await _dbContext.Tickets
                .FromSqlRaw("SELECT * FROM \"tickets\" WHERE \"id\" = {0} FOR UPDATE", ticketReservation.Id)
                .SingleOrDefaultAsync(t => t.Id == ticketReservation.Id);

            if (tickets == null)
            {
                throw new NotFoundException("Ticket", ticketReservation.Id);
            }

            tickets.ReduceTickets(ticketReservation.Quantity);

            _dbContext.Reservations.Add(newReservation);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    /// <summary>
    /// Removes a persisted reservation from the database and marks it as cancelled.
    /// </summary>
    public async Task<bool> ReleaseReservationFromDatabaseAsync(Guid userId, Guid ticketId, bool isCleanUp)
    {
        var now = DateTime.UtcNow;

        IQueryable<Reservation> query;

        if (isCleanUp)
        {
            // Clean up EXPIRED reservations only
            query = _dbContext.Reservations
                .Where(r =>
                    r.UserId == userId &&
                    r.ExpiresAt <= now &&
                    r.Items.Any(i => i.TicketId == ticketId));
        }
        else
        {
            // Manual release of ACTIVE, TEMPORARY reservations
            query = _dbContext.Reservations
                .Where(r =>
                    r.UserId == userId &&
                    r.ExpiresAt > now &&
                    r.Status == ReservationStatus.Temporary &&
                    r.Items.Any(i => i.TicketId == ticketId));
        }

        var reservations = await query
            .Include(r => r.Items)
            .ThenInclude(i => i.SeatAssignments)
            .ToListAsync();

        if (!reservations.Any())
            return false;

        foreach (var reservation in reservations)
        {
            reservation.MarkCancelled();

            foreach (var item in reservation.Items.Where(i => i.TicketId == ticketId))
            {
                var ticketEntity = await _dbContext.Tickets
                    .FromSqlRaw("SELECT * FROM \"tickets\" WHERE \"id\" = {0} FOR UPDATE", item.TicketId)
                    .SingleOrDefaultAsync();

                if (ticketEntity != null)
                {
                    ticketEntity.IncreaseTickets(item.Quantity);
                }
            }
        }

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ConfirmReservationInDatabaseAsync(Guid userId, Guid ticketId)
    {
        var reservation = await _dbContext.Reservations
            .Where(r => r.UserId == userId)
            .Include(r => r.Items)
            .ThenInclude(i => i.SeatAssignments)
            .FirstOrDefaultAsync(r => r.Items.Any(i => i.TicketId == ticketId && r.Status == ReservationStatus.Temporary));

        if (reservation == null)
            return false;

        reservation.MarkConfirmed();

        // Note: No need to reduce ticket inventory here, as it was already reduced when the reservation was created.

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsTicketReservedInDatabaseAsync(Guid userId, Guid ticketId)
    {
        var now = DateTime.UtcNow;

        var isReserved = await _dbContext.Reservations
            .Where(r =>
                r.UserId == userId &&
                r.Status == ReservationStatus.Temporary &&
                r.ExpiresAt > now &&
                r.Items.Any(i => i.TicketId == ticketId))
            .AnyAsync();

        return isReserved;
    }
}