using Microsoft.EntityFrameworkCore;
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

    public DeleteCouponCommandHandler(IApplicationDbContext context)
    {
        _context = context;
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
        return true;
    }
}