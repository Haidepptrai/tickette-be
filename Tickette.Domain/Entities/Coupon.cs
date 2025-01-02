using Tickette.Domain.Common;
using Tickette.Domain.Enums;

namespace Tickette.Domain.Entities;

public sealed class Coupon : BaseEntity
{
    public Guid EventId { get; set; }

    public string Code { get; private set; }

    public decimal DiscountValue { get; private set; }

    public DiscountType DiscountType { get; private set; }

    public DateTime ExpiryDate { get; private set; }

    public bool IsActive { get; private set; }

    public Event Event { get; set; }

    private Coupon(Guid eventId, string code, decimal discountValue, DiscountType discountType, DateTime expiryDate)
    {
        if (discountValue <= 0)
        {
            throw new ArgumentException("Discount value must be greater than zero.", nameof(discountValue));
        }

        EventId = eventId;
        Code = code.ToUpper();
        DiscountValue = discountValue;
        DiscountType = discountType;
        ExpiryDate = expiryDate;
        IsActive = true;
    }

    public static Coupon CreateCoupon(Guid eventId, string code, decimal discountValue, DiscountType discountType, DateTime expiryDate)
    {
        return new Coupon(eventId, code, discountValue, discountType, expiryDate);
    }

    public decimal CalculateDiscount(decimal originalPrice)
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

    public void Deactivate()
    {
        IsActive = false;
    }
}