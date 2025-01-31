namespace Tickette.Application.Features.Orders.Common;

public record TicketReservation
{
    public Guid TicketId { get; set; }

    public int Quantity { get; set; }
}