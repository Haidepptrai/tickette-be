namespace Tickette.Application.Features.Tickets.Common;

public class TicketOrderItemDto
{
    public Guid OrderId { get; set; }
    public Guid TicketId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}