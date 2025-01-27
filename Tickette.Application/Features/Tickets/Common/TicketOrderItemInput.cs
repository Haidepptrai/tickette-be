namespace Tickette.Application.Features.Tickets.Common;

public class TicketOrderItemInput
{
    public Guid TicketId { get; set; }
    public int Quantity { get; set; }
    public List<Guid>? SelectedSeats { get; set; } // List of selected seat IDs
}