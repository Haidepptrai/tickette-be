using Tickette.Application.Features.Orders.Common;

namespace Tickette.Application.Common.Interfaces.Redis;

public interface IReservationService
{
    Task<bool> ReserveTicketsAsync(Guid userId, ICollection<TicketReservation> tickets);
    Task<bool> ValidateReservationAsync(Guid userId, TicketReservation ticketReservationInfo);
    Task<bool> ReleaseReservationAsync(Guid userId, TicketReservation ticketId);
    Task<bool> FinalizeSeatReservationAsync(Guid userId, TicketReservation ticketReservationInfo);
}