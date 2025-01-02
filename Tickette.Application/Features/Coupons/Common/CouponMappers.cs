using Tickette.Application.Features.Coupons.Command.CreateCoupon;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Coupons.Common;

public static class CouponMappers
{
    public static CreateCouponResponse ToCreateCouponResponse(this Coupon coupon)
    {
        return new CreateCouponResponse(coupon.Code, coupon.DiscountValue, coupon.DiscountType, coupon.ExpiryDate);
    }

}