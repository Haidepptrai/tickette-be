using Microsoft.Extensions.Configuration;
using Stripe;
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


    public async Task<string> CreatePaymentIntentAsync(Payment payment)
    {
        var service = new PaymentIntentService();
        var options = new PaymentIntentCreateOptions
        {
            Amount = payment.Amount,
            Currency = payment.Currency,
            PaymentMethodTypes = new List<string> { "card" }
        };

        var paymentIntent = await service.CreateAsync(options);

        return paymentIntent.ClientSecret;
    }
}