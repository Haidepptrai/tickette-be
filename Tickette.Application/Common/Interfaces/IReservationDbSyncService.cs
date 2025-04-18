using Tickette.Application.Features.Orders.Common;

namespace Tickette.Application.Common.Interfaces;

public interface IReservationDbSyncService
{
    /// <summary>
    /// Persists a reserved ticket and its details to the database.
    /// </summary>
    /// <param name="userId">The ID of the user who made the reservation.</param>
    /// <param name="ticketReservation">Ticket and seat details.</param>
    Task<bool> PersistReservationAsync(Guid userId, TicketReservation ticketReservation);

    /// <summary>
    /// Removes a persisted reservation from the database and marks it as cancelled.
    /// </summary>
    /// <param name="userId">The ID of the user who made the reservation.</param>
    /// <param name="ticketId">The ticket identifier to release.</param>
    /// <param name="isCleanUp">Depend on this state, the check for ExpiresAt differently</param>
    /// <returns>True if any reservation was released; false otherwise.</returns>
    Task<bool> ReleaseReservationFromDatabaseAsync(Guid userId, Guid ticketId, bool isCleanUp);

    /// <summary>
    /// Marks a reservation as confirmed in the database.
    /// </summary>
    /// <param name="userId">The ID of the user who made the reservation.</param>
    /// <param name="ticketId">The ticket identifier to confirm.</param>
    /// <returns>True if the reservation was confirmed; false otherwise.</returns>
    Task<bool> ConfirmReservationInDatabaseAsync(Guid userId, Guid ticketId);

    /// <summary>
    /// Validates if a ticket is currently reserved (not expired, not cancelled) in the database.
    /// </summary>
    /// <param name="userId">The ID of the user to check the reservation for.</param>
    /// <param name="ticketId">The ticket ID to validate.</param>
    /// <returns>True if the ticket is actively reserved; false otherwise.</returns>
    Task<bool> IsTicketReservedInDatabaseAsync(Guid userId, Guid ticketId);

}