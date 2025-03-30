using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;

namespace Tickette.Application.Features.Coupons.Command.Client.DeleteCoupon;

public record DeleteCouponCommand
{
    public Guid EventId { get; init; }

    public string Code { get; init; }
}

public class DeleteCouponCommandHandler : ICommandHandler<DeleteCouponCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public DeleteCouponCommandHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(DeleteCouponCommand command, CancellationToken cancellation)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(cp => cp.Code == command.Code && cp.EventId == command.EventId, cancellation);

        if (coupon is null)
        {
            throw new NotFoundException("Coupon", command.Code);
        }

        coupon.SoftDeleteEntity();

        await _context.SaveChangesAsync(cancellation);

        _cacheService.RemoveCacheValue(InMemoryCacheKey.CouponList(command.EventId));
        return true;
    }
}