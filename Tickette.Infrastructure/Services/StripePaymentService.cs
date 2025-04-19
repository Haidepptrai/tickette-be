using Microsoft.Extensions.Configuration;
using Stripe;
using Tickette.Application.Common;
using Tickette.Application.Common.Interfaces.Stripe;
using Tickette.Domain.Entities;
using Tickette.Infrastructure.Helpers;

namespace Tickette.Infrastructure.Services;

public class StripePaymentService : IPaymentService
{
    public StripePaymentService(IConfiguration configuration)
    {
        var secretKey = configuration["Stripe:SecretKey"] ?? throw new ArgumentNullException("Stripe API Key is missing.");
        StripeConfiguration.ApiKey = secretKey;
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(Payment payment)
    {
        var service = new PaymentIntentService();
        var options = new PaymentIntentCreateOptions
        {
            Amount = StripeCurrencyHelper.ToStripeAmount(payment.Amount, payment.Currency),
            Currency = payment.Currency,
            PaymentMethodTypes = new List<string> { "card" },
            TransferData = new PaymentIntentTransferDataOptions
            {
                Destination = payment.EventOwnerStripeId
            },
            ApplicationFeeAmount = 1
        };

        var paymentIntent = await service.CreateAsync(options);

        var result = new PaymentIntentResult
        {
            ClientSecret = paymentIntent.ClientSecret,
            PaymentIntentId = paymentIntent.Id
        };

        return result;
    }

    public async Task<PaymentIntentResult> UpdatePaymentIntentAsync(string paymentIntentId, decimal newAmount)
    {
        if (string.IsNullOrWhiteSpace(paymentIntentId))
            throw new ArgumentException("Payment Intent ID cannot be null or empty.", nameof(paymentIntentId));

        if (newAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(newAmount), "Amount must be greater than zero.");

        var service = new PaymentIntentService();

        try
        {
            // Get the current PaymentIntent status
            var currentIntent = await service.GetAsync(paymentIntentId);

            // Check if the payment intent is in a state that allows updates
            if (currentIntent.Status != "requires_payment_method" &&
                currentIntent.Status != "requires_confirmation")
            {
                throw new InvalidOperationException("Cannot update payment intent that has already been processed.");
            }

            // Prepare the update options
            var options = new PaymentIntentUpdateOptions
            {
                Amount = StripeCurrencyHelper.ToStripeAmount(newAmount, currentIntent.Currency),
                Currency = currentIntent.Currency, // Use the same currency as the original intent
            };

            // Update the payment intent
            var updatedIntent = await service.UpdateAsync(paymentIntentId, options);

            // Return the updated payment intent details
            return new PaymentIntentResult
            {
                ClientSecret = updatedIntent.ClientSecret,
                PaymentIntentId = updatedIntent.Id
            };
        }
        catch (StripeException ex)
        {
            // Handle Stripe-specific exceptions
            if (ex.StripeError != null)
            {
                throw new InvalidOperationException($"Stripe Error: {ex.StripeError.Message}");
            }

            // Handle generic Stripe exceptions
            throw new InvalidOperationException("An error occurred while updating the payment intent.", ex);
        }
        catch (Exception ex)
        {
            // Handle any other exceptions
            throw new InvalidOperationException("An unexpected error occurred.", ex);
        }
    }
}