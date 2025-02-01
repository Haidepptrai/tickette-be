using Microsoft.Extensions.Configuration;
using Stripe;
using Tickette.Application.Common;
using Tickette.Application.Common.Interfaces.Stripe;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Services;

public class StripePaymentService : IPaymentService
{
    private readonly string _secretKey;

    public StripePaymentService(IConfiguration configuration)
    {
        _secretKey = configuration["Stripe:SecretKey"] ?? throw new ArgumentNullException("Stripe API Key is missing.");
        StripeConfiguration.ApiKey = _secretKey;
    }


    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(Payment payment)
    {
        var service = new PaymentIntentService();
        var options = new PaymentIntentCreateOptions
        {
            Amount = payment.Amount,
            Currency = payment.Currency,
            PaymentMethodTypes = new List<string> { "card" }
        };

        var paymentIntent = await service.CreateAsync(options);

        var result = new PaymentIntentResult
        {
            ClientSecret = paymentIntent.ClientSecret,
            PaymentIntentId = paymentIntent.Id
        };

        return result;
    }

    public async Task<PaymentIntentResult> UpdatePaymentIntentAsync(string paymentIntentId, long newAmount)
    {
        var service = new PaymentIntentService();
        var options = new PaymentIntentUpdateOptions
        {
            Amount = newAmount
        };
        var paymentIntent = await service.UpdateAsync(paymentIntentId, options);

        var result = new PaymentIntentResult
        {
            ClientSecret = paymentIntent.ClientSecret,
            PaymentIntentId = paymentIntent.Id
        };

        return result;
    }
}