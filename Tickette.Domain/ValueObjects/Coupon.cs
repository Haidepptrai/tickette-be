using Tickette.Domain.Common;

namespace Tickette.Domain.ValueObjects;

public class Coupon : ValueObject
{
    public string Code { get; private set; }

    public decimal DiscountAmount { get; private set; }

    public decimal? DiscountPercentage { get; private set; }

    public DateTime ExpiryDate { get; private set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}