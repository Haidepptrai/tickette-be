using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Stripe;
using Tickette.Application.Features.Orders.Common;

namespace Tickette.Application.Features.Orders.Command.ApplyCouponToOrder;

public record ApplyCouponToOrderCommand
{
    public Guid OrderId { get; init; }

    public string CouponCode { get; init; }

    public string PaymentIntentId { get; init; }
}

public class ApplyCouponToOrderCommandHandler : ICommandHandler<ApplyCouponToOrderCommand, ApplyCouponToOrderResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentService _paymentService;

    public ApplyCouponToOrderCommandHandler(IApplicationDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task<ApplyCouponToOrderResponse> Handle(ApplyCouponToOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .SingleOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order == null)
        {
            throw new Exception("Not found order");
        }

        if (order.CouponId != null)
        {
            throw new InvalidOperationException("Order already has a coupon applied.");
        }

        var coupon = await _context.Coupons
            .SingleOrDefaultAsync(c => c.Code == command.CouponCode, cancellationToken);

        if (coupon == null)
        {
            throw new Exception("Coupon not found!");
        }

        order.ApplyCoupon(coupon);
        var paymentServiceResult = await _paymentService.UpdatePaymentIntentAsync(command.PaymentIntentId, order.TotalPrice);

        var result = new ApplyCouponToOrderResponse
        {
            TotalPrice = order.TotalPrice,
            ClientSecret = paymentServiceResult.ClientSecret,
            PaymentIntentId = paymentServiceResult.PaymentIntentId
        };


        await _context.SaveChangesAsync(cancellationToken);
        return result;
    }
}