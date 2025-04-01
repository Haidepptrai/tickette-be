using Tickette.Application.Features.Orders.Command.ReserveTicket;

namespace Tickette.Application.Common.Interfaces.Messaging;

public interface ITicketReservationPublisher
{
    Task<bool> PublishAsync(ReserveTicketCommand reservation);
}