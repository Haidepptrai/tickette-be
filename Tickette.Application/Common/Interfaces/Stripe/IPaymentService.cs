using Tickette.Domain.Entities;

namespace Tickette.Application.Common.Interfaces.Stripe;

public interface IPaymentService
{
    Task<string> CreatePaymentIntentAsync(Payment payment);
}