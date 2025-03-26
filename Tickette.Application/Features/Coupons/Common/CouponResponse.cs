using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Coupons.Common;

public record CouponResponse
{
    public string Code { get; init; }
    public decimal DiscountValue { get; init; }
    public DiscountType DiscountType { get; init; }
    public DateTime StartValidDate { get; init; }
    public DateTime ExpiryDate { get; init; }

    public CouponResponse(string code, decimal discountValue, DiscountType discountType, DateTime startValidDate, DateTime expiryDate)
    {
        Code = code;
        DiscountValue = discountValue;
        DiscountType = discountType;
        StartValidDate = startValidDate;
        ExpiryDate = expiryDate;
    }
}