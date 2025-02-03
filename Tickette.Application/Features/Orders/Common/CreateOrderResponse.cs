namespace Tickette.Application.Features.Orders.Common;

public record CreateOrderResponse
{
    public Guid OrderId { get; init; }
}