namespace Tickette.Domain.Entities;

public class Payment
{
    public decimal Amount { get; set; }

    public string Currency { get; set; }

    private Payment(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Payment Create(decimal amount, string currency = "usd")
    {
        return new Payment(amount, currency);
    }
}