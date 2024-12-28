namespace Tickette.Application.Features.Events.Common;

public class TicketDto
{
    public Guid TicketId { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public int TotalTickets { get; set; }

    public int MinTicketsPerOrder { get; set; }

    public int MaxTicketsPerOrder { get; set; }

    public DateTime SaleStartTime { get; set; }

    public DateTime SaleEndTime { get; set; }

    public DateTime EventStartTime { get; set; }

    public DateTime EventEndTime { get; set; }

    public string Description { get; set; }

    public string? TicketImage { get; set; }
}