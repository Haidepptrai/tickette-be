using Tickette.Domain.Entities;

namespace Tickette.Application.Common.Interfaces.Stripe;

public interface IPaymentService
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(Payment payment);

    Task<PaymentIntentResult> UpdatePaymentIntentAsync(string paymentIntentId, decimal newAmount);
}