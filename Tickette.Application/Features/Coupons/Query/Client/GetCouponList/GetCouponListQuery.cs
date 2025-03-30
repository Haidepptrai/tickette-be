using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Coupons.Common;

namespace Tickette.Application.Features.Coupons.Query.Client.GetCouponList;

public record GetCouponListQuery
{
    public Guid EventId { get; init; }
}

public class GetCouponListQueryHandler : IQueryHandler<GetCouponListQuery, IEnumerable<CouponResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetCouponListQueryHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<CouponResponse>> Handle(GetCouponListQuery query, CancellationToken cancellation)
    {
        var cachedValue = _cacheService.GetCacheValue<IEnumerable<CouponResponse>>(
            InMemoryCacheKey.CouponList(query.EventId));

        if (cachedValue != null)
        {
            return cachedValue;
        }

        var coupons = await _context.Coupons
            .Where(cp => cp.EventId == query.EventId)
            .Select(cp => cp.ToCreateCouponResponse())
            .ToListAsync(cancellation);

        _cacheService.SetCacheValue(InMemoryCacheKey.CouponList(query.EventId), coupons);

        return coupons;
    }
}