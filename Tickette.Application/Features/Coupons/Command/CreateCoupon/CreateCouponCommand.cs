using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Coupons.Common;
using Tickette.Application.Helpers;
using Tickette.Domain.Entities;
using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Coupons.Command.CreateCoupon;

public record CreateCouponCommand(Guid EventId, string Code, decimal DiscountValue, DiscountType DiscountType, DateTime ExpiryDate);

public class CreateCouponCommandHandler : ICommandHandler<CreateCouponCommand, ResponseDto<CreateCouponResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateCouponCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResponseDto<CreateCouponResponse>> Handle(CreateCouponCommand command, CancellationToken cancellation)
    {
        try
        {
            var coupon = Coupon.CreateCoupon(command.EventId, command.Code, command.DiscountValue, command.DiscountType, command.ExpiryDate);
            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync(cancellation);

            var couponResponse = coupon.ToCreateCouponResponse();

            return ResponseHandler.SuccessResponse(couponResponse, "Coupon Created Successfully");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ErrorResponse<CreateCouponResponse>(null, ex.Message);
        }
    }
}

