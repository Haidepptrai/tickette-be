namespace Tickette.Infrastructure.Helpers;

public static class StripeCurrencyHelper
{
    private static readonly HashSet<string> ZeroDecimalCurrencies = new()
    {
        "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga",
        "pyg", "rwf", "vnd", "vuv", "xaf", "xof", "xpf"
    };

    public static long ToStripeAmount(decimal amount, string currency)
    {
        currency = currency.ToLowerInvariant();

        if (ZeroDecimalCurrencies.Contains(currency))
        {
            return (long)Math.Round(amount);
        }

        return (long)Math.Round(amount * 100);
    }
}
