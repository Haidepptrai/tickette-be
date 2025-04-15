using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;
using Tickette.Domain.Common;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.CommitteeMembers.Command.RemoveCommitteeMember;

public record RemoveCommitteeMemberCommand
{
    public Guid EventId { get; init; }
    public Guid MemberId { get; init; }
}

public class RemoveCommitteeMemberCommandHandler : ICommandHandler<RemoveCommitteeMemberCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public RemoveCommitteeMemberCommandHandler(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Unit> Handle(RemoveCommitteeMemberCommand request, CancellationToken cancellationToken)
    {
        var entity =
            await _context.CommitteeMembers.SingleOrDefaultAsync(cm =>
                cm.EventId == request.EventId && cm.UserId == request.MemberId, cancellationToken);

        if (entity == null)
            throw new NotFoundException(nameof(CommitteeMember), request.MemberId);

        _context.CommitteeMembers.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        // Remove from cache
        if (_cache.TryGetValue(InMemoryCacheKey.CommitteeMemberOfEvent(request.EventId), out List<CommitteeMember>? committeeMembers))
        {
            committeeMembers?.Remove(entity!);
            _cache.Set(InMemoryCacheKey.CommitteeMemberOfEvent(request.EventId), committeeMembers);
        }

        return Unit.Value;
    }
}