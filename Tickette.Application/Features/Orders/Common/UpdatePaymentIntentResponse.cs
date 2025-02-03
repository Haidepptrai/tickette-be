namespace Tickette.Application.Features.Orders.Common;

public record UpdatePaymentIntentResponse
{
    // Nullable since if final price is 0, we don't need to update the payment intent
    public string? PaymentIntentId { get; init; }

    public string? ClientSecret { get; init; }

    public decimal TotalPrice { get; init; }
}