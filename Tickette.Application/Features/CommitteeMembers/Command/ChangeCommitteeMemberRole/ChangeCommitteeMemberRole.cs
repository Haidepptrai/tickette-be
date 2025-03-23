using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;

namespace Tickette.Application.Features.CommitteeMembers.Command.ChangeCommitteeMemberRole;

public record ChangeCommitteeMemberRoleCommand
{
    public Guid MemberId { get; init; }
    public Guid RoleId { get; init; }
    public Guid EventId { get; init; }
}

public class ChangeCommitteeMemberRoleCommandHandler : ICommandHandler<ChangeCommitteeMemberRoleCommand, object>
{
    private readonly IApplicationDbContext _context;

    public ChangeCommitteeMemberRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> Handle(ChangeCommitteeMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CommitteeMembers.FirstOrDefaultAsync(cm => cm.UserId == request.MemberId && cm.EventId == request.RoleId, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException("Committee Member", request.MemberId);
        }

        entity.ChangeRole(request.RoleId);

        await _context.SaveChangesAsync(cancellationToken);

        return null!;
    }
}