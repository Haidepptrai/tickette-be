namespace Tickette.Application.Features.Orders.Common;

public record OrderedTicketListDto
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public required string EventName { get; set; }

    public required string VenueName { get; set; }

    public DateTime TicketStartDate { get; set; }

    public DateTime TicketEndDate { get; set; }
}