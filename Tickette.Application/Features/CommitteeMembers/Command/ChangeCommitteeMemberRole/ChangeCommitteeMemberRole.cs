using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;

namespace Tickette.Application.Features.CommitteeMembers.Command.ChangeCommitteeMemberRole;

public record ChangeCommitteeMemberRoleCommand
{
    public Guid CommitteeMemberId { get; init; }
    public Guid CommitteeRoleId { get; init; }
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
        var entity = await _context.CommitteeMembers.FindAsync([request.CommitteeMemberId], cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException("Committee Member Not Found");
        }

        entity.ChangeRole(request.CommitteeRoleId);

        await _context.SaveChangesAsync(cancellationToken);

        return null!;
    }
}