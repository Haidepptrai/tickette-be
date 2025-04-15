using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;
using Tickette.Domain.Common;

namespace Tickette.Application.Features.CommitteeMembers.Command.ChangeCommitteeMemberRole;

public record ChangeCommitteeMemberRoleCommand
{
    public Guid MemberId { get; init; }
    public Guid RoleId { get; init; }
    public Guid EventId { get; init; }
}

public class ChangeCommitteeMemberRoleCommandHandler : ICommandHandler<ChangeCommitteeMemberRoleCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public ChangeCommitteeMemberRoleCommandHandler(IApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Unit> Handle(ChangeCommitteeMemberRoleCommand request, CancellationToken cancellationToken)
    {
        // cm.Id == request.MemberId is a stupid move
        // I should change it to composite key
        var entity = await _context.CommitteeMembers.FirstOrDefaultAsync(cm => cm.UserId == request.MemberId && cm.EventId == request.EventId, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException("Committee Member", request.MemberId);
        }

        entity.ChangeRole(request.RoleId);

        await _context.SaveChangesAsync(cancellationToken);

        _cacheService.RemoveCacheValue(InMemoryCacheKey.CommitteeMemberOfEvent(request.EventId));

        return Unit.Value;
    }
}