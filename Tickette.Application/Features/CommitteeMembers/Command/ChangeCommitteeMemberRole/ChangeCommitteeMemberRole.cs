using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Domain.ValueObjects;

namespace Tickette.Application.Features.CommitteeMembers.Command.ChangeCommitteeMemberRole;

public record ChangeCommitteeMemberRoleCommand
{
    public Guid CommitteeMemberId { get; init; }
    public CommitteeRole Role { get; init; }
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

        entity.UpdateRole(request.Role);

        await _context.SaveChangesAsync(cancellationToken);

        return null!;
    }
}