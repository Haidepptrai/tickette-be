using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;

namespace Tickette.Application.Features.CommitteeMembers.Command.RemoveCommitteeMember;

public record RemoveCommitteeMemberCommand
{
    public Guid Id { get; init; }
}

public class RemoveCommitteeMemberCommandHandler : ICommandHandler<RemoveCommitteeMemberCommand, object>
{
    private readonly IApplicationDbContext _context;

    public RemoveCommitteeMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> Handle(RemoveCommitteeMemberCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CommitteeMembers.FindAsync([request.Id], cancellationToken);

        _context.CommitteeMembers.Remove(entity!);

        await _context.SaveChangesAsync(cancellationToken);

        return null!;
    }
}