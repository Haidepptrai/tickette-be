using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Tickets.Common;

public static class TicketMappers
{
    public static TicketOrderItem ToCreateTicketOrderItemDto(this TicketOrderItemDto ticketOrderItem)
    {
        return new TicketOrderItem
        (
            ticketOrderItem.TicketId,
            ticketOrderItem.Quantity,
            ticketOrderItem.Price
        );

    }
}