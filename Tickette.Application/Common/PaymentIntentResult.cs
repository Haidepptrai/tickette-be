namespace Tickette.Application.Common;

public record PaymentIntentResult
{
    public string PaymentIntentId { get; init; }

    public string ClientSecret { get; init; }
}