using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Coupons.Common;
using Tickette.Application.Wrappers;
using Tickette.Domain.Entities;
using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Coupons.Command.Client.CreateCoupon;

public record CreateCouponCommand(Guid EventId, string Code, decimal DiscountValue, DiscountType DiscountType, DateTime StartValidDate, DateTime ExpiryDate);

public class CreateCouponCommandHandler : ICommandHandler<CreateCouponCommand, ResponseDto<CouponResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateCouponCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResponseDto<CouponResponse>> Handle(CreateCouponCommand command, CancellationToken cancellation)
    {
        // Check if the coupon code already exists in the database
        var existingCoupon = await _context.Coupons
            .AnyAsync(coupon => coupon.Code.ToUpper() == command.Code.ToUpper() && coupon.EventId == command.EventId, cancellation);

        if (existingCoupon)
        {
            // Return an error response if the coupon code already exists
            return ResponseHandler.ErrorResponse<CouponResponse>(null, "Coupon code already exists for this event.");
        }

        var coupon = Coupon.CreateCoupon(command.EventId, command.Code, command.DiscountValue, command.DiscountType, command.StartValidDate, command.ExpiryDate);
        _context.Coupons.Add(coupon);

        await _context.SaveChangesAsync(cancellation);

        var couponResponse = coupon.ToCreateCouponResponse();

        return ResponseHandler.SuccessResponse(couponResponse, "Coupon Created Successfully");
    }
}

