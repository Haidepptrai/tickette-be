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

    public static Payment Create(decimal amount, string eventOwnerStripeId, string currency = "usd")
    {
        return new Payment(amount, currency, eventOwnerStripeId);
    }
}