using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;
using Tickette.Application.Features.Coupons.Common;
using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Coupons.Command.Client.UpdateCoupon;

public record UpdateCouponCommand
{
    public Guid EventId { get; init; }
    public string Code { get; init; }
    public string OriginalCode { get; init; }
    public decimal DiscountValue { get; init; }
    public DiscountType DiscountType { get; init; }
    public DateTime StartValidDate { get; init; }
    public DateTime ExpiryDate { get; init; }
}

public class UpdateCouponCommandHandler : ICommandHandler<UpdateCouponCommand, CouponResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public UpdateCouponCommandHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<CouponResponse> Handle(UpdateCouponCommand command, CancellationToken cancellation)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(cp => cp.Code == command.OriginalCode && cp.EventId == command.EventId, cancellation);

        if (coupon is null)
        {
            throw new NotFoundException("Coupon", command.Code);
        }

        coupon.UpdateCouponInformation(command.Code, command.DiscountValue, command.DiscountType, command.StartValidDate, command.ExpiryDate);

        await _context.SaveChangesAsync(cancellation);

        _cacheService.RemoveCacheValue(InMemoryCacheKey.CouponList(command.EventId));

        return new CouponResponse(coupon.Code, coupon.DiscountValue, coupon.DiscountType, coupon.StartValidDate, coupon.ExpiryDate);
    }
}