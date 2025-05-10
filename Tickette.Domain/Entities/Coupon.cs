using Tickette.Domain.Common;
using Tickette.Domain.Common.Exceptions;
using Tickette.Domain.Enums;

namespace Tickette.Domain.Entities;

public sealed class Coupon : BaseEntity
{
    public Guid EventId { get; set; }

    public string Code { get; private set; }

    public decimal DiscountValue { get; private set; }

    public DiscountType DiscountType { get; private set; }

    public DateTime StartValidDate { get; private set; }

    public DateTime ExpiryDate { get; private set; }

    public bool IsActive { get; private set; }

    public Event Event { get; set; }

    private Coupon(Guid eventId, string code, decimal discountValue, DiscountType discountType, DateTime startValidDate, DateTime expiryDate)
    {
        EventId = eventId;
        Code = code.ToUpper();
        DiscountValue = discountValue;
        DiscountType = discountType;
        StartValidDate = startValidDate;
        ExpiryDate = expiryDate;
        IsActive = true;
    }

    public static Coupon CreateCoupon(Guid eventId, string code, decimal discountValue, DiscountType discountType, DateTime validFrom, DateTime expiryDate)
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

        return new Coupon(eventId, code, discountValue, discountType, validFrom, expiryDate);
    }

    public decimal CalculateDiscount(decimal originalPrice)
    {
        //if (!IsActive)
        //{
        //    throw new InvalidCouponException("The coupon is not active.");
        //}

        if (DateTime.UtcNow > ExpiryDate || DateTime.UtcNow < StartValidDate)
        {
            throw new InvalidCouponException("The coupon is not valid.");
        }

        return DiscountType switch
        {
            DiscountType.Flat => DiscountValue, // Flat discount
            DiscountType.Percentage => originalPrice * (DiscountValue / 100), // Percentage discount
            _ => throw new InvalidOperationException("Unknown discount type.")
        };
    }

    public decimal CalculateFinalPrice(decimal originalPrice)
    {
        return originalPrice - CalculateDiscount(originalPrice);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateCouponInformation(string code, decimal discountValue, DiscountType discountType, DateTime startValidDate,
        DateTime expiryDate)
    {
        if (!IsActive) throw new InvalidCouponException("Coupon is currently deactivated");

        if (DateTime.UtcNow > expiryDate)
        {
            throw new InvalidCouponException("Expiry date must be in the future.");
        }

        if (discountValue <= 0)
        {
            throw new ArgumentException("Discount value must be greater than zero.", nameof(discountValue));
        }

        Code = code;
        DiscountValue = discountValue;
        DiscountType = discountType;
        StartValidDate = startValidDate;
        ExpiryDate = expiryDate;
    }
}