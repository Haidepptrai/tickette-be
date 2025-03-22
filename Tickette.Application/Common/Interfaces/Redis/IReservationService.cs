using Tickette.Application.Features.Orders.Common;

namespace Tickette.Application.Common.Interfaces.Redis;

public interface IReservationService
{
    Task<bool> ReserveTicketsAsync(Guid userId, TicketReservation reservation);
    Task<bool> ValidateReservationAsync(Guid ticketId, Guid userId);
    Task<bool> ReleaseReservationAsync(Guid ticketId, Guid userId);
}