using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Coupons.Command.CreateCoupon;

public record CreateCouponResponse(string Code, decimal DiscountValue, DiscountType DiscountType, DateTime StartValidDate, DateTime ExpiryDate);