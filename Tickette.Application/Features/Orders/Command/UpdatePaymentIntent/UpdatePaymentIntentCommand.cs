using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Stripe;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common.Exceptions;

namespace Tickette.Application.Features.Orders.Command.UpdatePaymentIntent;

public record UpdatePaymentIntentCommand
{
    public string PaymentIntentId { get; init; }

    public decimal CurrentTotalPrice { get; init; }

    public string Coupon { get; init; }

    public Guid EventId { get; init; }
}

public class UpdatePaymentIntentCommandHandler : ICommandHandler<UpdatePaymentIntentCommand, UpdatePaymentIntentResponse>
{
    private readonly IPaymentService _paymentService;
    private readonly IApplicationDbContext _context;

    public UpdatePaymentIntentCommandHandler(IPaymentService paymentService, IApplicationDbContext context)
    {
        _paymentService = paymentService;
        _context = context;
    }

    public async Task<UpdatePaymentIntentResponse> Handle(UpdatePaymentIntentCommand command,
        CancellationToken cancellationToken)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code.ToUpper() == command.Coupon && c.EventId == command.EventId, cancellationToken);


        if (coupon == null)
        {
            throw new InvalidCouponException($"Invalid Coupon Code: {command.Coupon}");
        }

        var discountPrice = coupon.CalculateFinalPrice(command.CurrentTotalPrice);

        if (discountPrice <= 0)
        {
            return new UpdatePaymentIntentResponse
            {
                ClientSecret = null,
                PaymentIntentId = null,
                TotalPrice = discountPrice
            };
        }

        var paymentServiceResult =
            await _paymentService.UpdatePaymentIntentAsync(command.PaymentIntentId, discountPrice);

        return new UpdatePaymentIntentResponse
        {
            ClientSecret = paymentServiceResult.ClientSecret,
            PaymentIntentId = paymentServiceResult.PaymentIntentId,
            TotalPrice = discountPrice
        };

    }
}