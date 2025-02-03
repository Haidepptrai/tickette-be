namespace Tickette.Application.Features.Orders.Common;

public record TicketReservation
{
    public Guid Id { get; set; }

    public int Quantity { get; set; }
}