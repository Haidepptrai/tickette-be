namespace Tickette.Application.Features.EventDates.Common;

public class TicketForSeatMapDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }
}