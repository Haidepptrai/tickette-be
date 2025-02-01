using Tickette.Domain.Common;
using Tickette.Domain.Enums;

namespace Tickette.Domain.Entities;

public sealed class Coupon : BaseEntity
{
    public Guid EventId { get; set; }

    public string Code { get; private set; }

    public long DiscountValue { get; private set; }

    public DiscountType DiscountType { get; private set; }

    public DateTime ExpiryDate { get; private set; }

    public bool IsActive { get; private set; }

    public Event Event { get; set; }

    private Coupon(Guid eventId, string code, long discountValue, DiscountType discountType, DateTime expiryDate)
    {
        EventId = eventId;
        Code = code.ToUpper();
        DiscountValue = discountValue;
        DiscountType = discountType;
        ExpiryDate = expiryDate;
        IsActive = true;
    }

    public static Coupon CreateCoupon(Guid eventId, string code, long discountValue, DiscountType discountType, DateTime expiryDate)
    {
        //Conditional check
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Coupon code is required.", nameof(code));
        }

        if (DateTime.UtcNow > expiryDate)
        {
            throw new ArgumentException("Expiry date must be in the future.", nameof(expiryDate));
        }

        if (discountValue <= 0)
        {
            throw new ArgumentException("Discount value must be greater than zero.", nameof(discountValue));
        }

        return new Coupon(eventId, code, discountValue, discountType, expiryDate);
    }

    public long CalculateDiscount(long originalPrice)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("The coupon is not active.");
        }

        if (DateTime.UtcNow > ExpiryDate)
        {
            throw new InvalidOperationException("The coupon has expired.");
        }

        return DiscountType switch
        {
            DiscountType.Flat => DiscountValue, // Flat discount
            DiscountType.Percentage => originalPrice * (DiscountValue / 100), // Percentage discount
            _ => throw new InvalidOperationException("Unknown discount type.")
        };
    }

    public long CalculateFinalPrice(long originalPrice)
    {
        return originalPrice - CalculateDiscount(originalPrice);
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}