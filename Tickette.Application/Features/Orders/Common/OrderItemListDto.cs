namespace Tickette.Application.Features.Orders.Common;

public record OrderItemListDto
{
    public Guid Id { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public required OrderedTicketDto Tickets { get; set; }
}