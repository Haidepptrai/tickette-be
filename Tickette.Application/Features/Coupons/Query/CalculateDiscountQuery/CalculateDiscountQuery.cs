using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Coupons.Common;
using Tickette.Application.Helpers;

namespace Tickette.Application.Features.Coupons.Query.CalculateDiscountQuery;

public record CalculateDiscountQuery
{
    public required string CouponCode { get; set; }
    public required Guid EventId { get; set; }
    public required decimal CurrentPrice { get; set; }
}

public class CalculateDiscountQueryHandler : IQueryHandler<CalculateDiscountQuery, ResponseDto<PriceDiscountInformationDto>>
{
    private readonly IApplicationDbContext _context;

    public CalculateDiscountQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResponseDto<PriceDiscountInformationDto>> Handle(CalculateDiscountQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code.ToUpper() == request.CouponCode && c.EventId == request.EventId, cancellationToken);
            if (coupon == null)
            {
                return ResponseHandler.ErrorResponse<PriceDiscountInformationDto>(null, "Coupon is not valid.");
            }

            var discountAmount = coupon.CalculateDiscount(request.CurrentPrice);
            var finalPrice = coupon.CalculateFinalPrice(request.CurrentPrice);

            var priceDiscountInfo = new PriceDiscountInformationDto
            {
                DiscountAmount = discountAmount,
                FinalPrice = finalPrice
            };

            return ResponseHandler.SuccessResponse(priceDiscountInfo, "Retrieve Discount Successfully");
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
