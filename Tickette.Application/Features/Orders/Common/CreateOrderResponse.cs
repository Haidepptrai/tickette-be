namespace Tickette.Application.Features.Orders.Common;

public record CreateOrderResponse
{
    public Guid OrderId { get; init; }

    public string PaymentIntentId { get; init; }

    public string ClientSecret { get; init; }
}