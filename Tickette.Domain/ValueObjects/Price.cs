using Tickette.Domain.Common;

namespace Tickette.Domain.ValueObjects;

public class Price : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    private static readonly HashSet<string> SupportedCurrencies = new()
    {
        "USD", "VND"
    };

    private Price() { }

    public Price(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(amount));

        if (!SupportedCurrencies.Contains(currency))
            throw new ArgumentException($"Unsupported currency: {currency}", nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    // For equality, compare Amount and Currency
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    // Override ToString for display
    public override string ToString() => $"{Currency} {Amount:0.00}";

    // Operator overloading
    public static Price operator +(Price p1, Price p2)
    {
        if (p1.Currency != p2.Currency)
            throw new InvalidOperationException("Cannot add prices with different currencies.");

        return new Price(p1.Amount + p2.Amount, p1.Currency);
    }

    public static Price operator *(Price p, int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.");

        return new Price(p.Amount * quantity, p.Currency);
    }

    public static Price operator *(Price p, decimal multiplier)
    {
        if (multiplier < 0)
            throw new ArgumentException("Multiplier cannot be negative.");

        return new Price(p.Amount * multiplier, p.Currency);
    }
}
