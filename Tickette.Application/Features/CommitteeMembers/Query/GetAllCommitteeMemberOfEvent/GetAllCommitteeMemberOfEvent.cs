using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.CommitteeMembers.Common;
using Tickette.Application.Features.CommitteeMembers.Commond;

namespace Tickette.Application.Features.CommitteeMembers.Query.GetAllCommitteeMemberOfEvent;

public record GetAllCommitteeMemberOfEventQuery
{
    public Guid EventId { get; init; }
}

public class GetAllCommitteeMemberOfEventHandler : IQueryHandler<GetAllCommitteeMemberOfEventQuery, IEnumerable<CommitteeMemberDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCommitteeMemberOfEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CommitteeMemberDto>> Handle(GetAllCommitteeMemberOfEventQuery request, CancellationToken cancellationToken)
    {
        var entities = await _context.CommitteeMembers
            .Where(x => x.EventId == request.EventId)
            .Include(x => x.User)
            .Include(x => x.CommitteeRole)
            .ToListAsync(cancellationToken);

        var result = entities.Select(c => c.ToCommitteeMemberDto()).ToList();

        return result;
    }
}