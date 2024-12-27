using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Domain.Entities;
using Tickette.Domain.ValueObjects;

namespace Tickette.Application.Features.CommitteeMembers.Command.AddCommitteeMember;

public record AddCommitteeMemberCommand
{
    public Guid UserId { get; init; }
    public CommitteeRole Role { get; init; }
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
        var entity = new CommitteeMember(request.UserId, request.Role, request.EventId);

        _context.CommitteeMembers.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return null!;
    }
}