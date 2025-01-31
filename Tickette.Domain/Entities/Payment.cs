namespace Tickette.Domain.Entities;

public class Payment
{
    public long Amount { get; set; }

    public string Currency { get; set; }

    private Payment(long amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Payment Create(long amount, string currency = "usd")
    {
        return new Payment(amount, currency);
    }
}