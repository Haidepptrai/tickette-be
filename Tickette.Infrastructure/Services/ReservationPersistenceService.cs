using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Entities;
using Tickette.Domain.Enums;

namespace Tickette.Infrastructure.Services;

/// <summary>
/// This class is responsible for persisting reservations to the database.
/// Since Redis could potentially crash
/// We need to ensure that the reservation is saved in the database to recover it later.
/// </summary>
public class ReservationPersistenceService
{
    private readonly IApplicationDbContext _dbContext;

    public ReservationPersistenceService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Persists a reserved ticket and its details to the database.
    /// </summary>
    /// <param name="userId">The ID of the user who made the reservation</param>
    /// <param name="ticketReservation">Ticket and seat details</param>
    /// <returns></returns>
    public async Task PersistReservationAsync(Guid userId, TicketReservation ticketReservation)
    {
        var reservation = new Reservation(userId, DateTime.UtcNow.AddMinutes(15));

        reservation.AddItem(
            ticketId: ticketReservation.Id,
            quantity: ticketReservation.Quantity,
            hasAssignedSeats: ticketReservation.SeatsChosen != null
        );

        if (ticketReservation.SeatsChosen != null)
        {
            var item = reservation.Items.First(); // Only one item in this reservation
            foreach (var seat in ticketReservation.SeatsChosen)
            {
                item.AssignSeat(seat.RowName, seat.SeatNumber);
            }
        }

        // Decrease ticket inventory
        var eventEntity = await _dbContext.Tickets.FindAsync(ticketReservation.Id);
        if (eventEntity != null)
        {
            eventEntity.ReduceTickets(ticketReservation.Quantity);
        }

        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Removes a persisted reservation from the database and marks it as cancelled.
    /// </summary>
    public async Task<bool> ReleaseReservationFromDatabaseAsync(Guid userId, Guid ticketId)
    {
        var reservation = await _dbContext.Reservations
            .Where(r => r.UserId == userId)
            .Include(r => r.Items)
            .ThenInclude(i => i.SeatAssignments)
            .FirstOrDefaultAsync(r => r.Items.Any(i => i.TicketId == ticketId));

        if (reservation == null)
            return false;

        reservation.MarkCancelled(); // Sets status to "Cancelled"

        // Restore ticket inventory
        foreach (var item in reservation.Items.Where(i => i.TicketId == ticketId))
        {
            var eventEntity = await _dbContext.Tickets.FindAsync(item.TicketId);
            if (eventEntity != null)
            {
                eventEntity.IncreaseTickets(item.Quantity);
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

        // Reduce ticket inventory
        foreach (var item in reservation.Items.Where(i => i.TicketId == ticketId))
        {
            var eventEntity = await _dbContext.Tickets.FindAsync(item.TicketId);
            if (eventEntity != null)
            {
                eventEntity.ReduceTickets(item.Quantity);
            }
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }
}