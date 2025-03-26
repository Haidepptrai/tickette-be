using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.CommitteeMembers.Common;
using Tickette.Application.Features.Events.Common.Client;

namespace Tickette.Application.Features.CommitteeMembers.Query.GetAllCommitteeMemberOfEvent;

public record GetAllCommitteeMemberOfEventQuery
{
    public Guid EventId { get; init; }
}

public class GetAllCommitteeMemberOfEventHandler : IQueryHandler<GetAllCommitteeMemberOfEventQuery, GetAllCommitteeMemberOfEventResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityServices _identityServices;

    public GetAllCommitteeMemberOfEventHandler(IApplicationDbContext context, IIdentityServices identityServices)
    {
        _context = context;
        _identityServices = identityServices;
    }

    public async Task<GetAllCommitteeMemberOfEventResponse> Handle(GetAllCommitteeMemberOfEventQuery request, CancellationToken cancellationToken)
    {
        var entities = await _context.CommitteeMembers
            .Where(x => x.EventId == request.EventId)
            .Include(x => x.User)
            .Include(x => x.CommitteeRole)
            .ToListAsync(cancellationToken);

        var roles = await _identityServices.GetRoleAllRoles();

        var result = entities.ToCommitteeMemberResponse(roles);

        return result;
    }
}