namespace Tickette.Application.Features.Orders.Common;

public record OrderedTicketGroupListDto
{
    public Guid Id { get; set; }

    public string EventBanner { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Address { get; set; }

    public List<OrderItemListDto> OrderItems { get; set; } = [];
}