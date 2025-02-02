namespace Tickette.Application.Features.Orders.Common;

public record ApplyCouponToOrderResponse
{
    public string PaymentIntentId { get; init; }

    public string ClientSecret { get; init; }

    public decimal TotalPrice { get; init; }
}