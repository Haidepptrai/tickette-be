namespace Tickette.Application.Features.Orders.Common;

public record OrderedTicketGroupListDto
{
    public Guid OrderId { get; set; }
    public List<OrderedTicketListDto> Tickets { get; set; } = [];
}