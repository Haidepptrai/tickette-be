using Microsoft.EntityFrameworkCore;
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

    public GetCouponListQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CouponResponse>> Handle(GetCouponListQuery query, CancellationToken cancellation)
    {
        var coupons = await _context.Coupons
            .Where(cp => cp.EventId == query.EventId)
            .Select(cp => cp.ToCreateCouponResponse())
            .ToListAsync(cancellation);
        return coupons;
    }
}