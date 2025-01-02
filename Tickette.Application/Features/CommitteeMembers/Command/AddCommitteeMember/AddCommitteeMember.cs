using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.CommitteeMembers.Command.AddCommitteeMember;

public record AddCommitteeMemberCommand
{
    public Guid UserId { get; init; }
    public Guid CommitteeMemberRoleId { get; init; }
    public Guid EventId { get; init; }
}

public class AddCommitteeMemberCommandHandler : ICommandHandler<AddCommitteeMemberCommand, object>
{
    private readonly IApplicationDbContext _context;

    public AddCommitteeMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> Handle(AddCommitteeMemberCommand request, CancellationToken cancellationToken)
    {
        var entity = new CommitteeMember(request.UserId, request.CommitteeMemberRoleId, request.EventId);

        _context.CommitteeMembers.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return null!;
    }
}