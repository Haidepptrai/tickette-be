using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Coupons.Common;

public static class CouponMappers
{
    public static CouponResponse ToCreateCouponResponse(this Coupon coupon)
    {
        return new CouponResponse(coupon.Code, coupon.DiscountValue, coupon.DiscountType, coupon.StartValidDate, coupon.ExpiryDate);
    }

}