namespace Tickette.Application.Features.Coupons.Common;

public record PriceDiscountInformationDto
{
    public decimal DiscountAmount { get; set; }

    public decimal FinalPrice { get; set; }
}