using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Tickets.Common;

public static class TicketMappers
{
    public static OrderItem ToCreateTicketOrderItemDto(this TicketOrderItemDto ticketOrderItem)
    {
        return new OrderItem
        (
            ticketOrderItem.TicketId,
            ticketOrderItem.Quantity
        );

    }
}