using Tickette.Domain.ValueObjects;

namespace Tickette.Domain.Entities;

public sealed class Payment
{
    public decimal Amount { get; init; }

    public string Currency { get; init; }

    public string EventOwnerStripeId { get; init; }

    private Payment(decimal amount, string currency, string eventOwnerStripeId)
    {
        Amount = amount;
        Currency = currency;
        EventOwnerStripeId = eventOwnerStripeId;
    }

    public static Payment Create(Price price, string eventOwnerStripeId)
    {
        return new Payment(price.Amount, price.Currency, eventOwnerStripeId);
    }
}