using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.Constants;
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
    private readonly ICacheService _cacheService;

    public GetAllCommitteeMemberOfEventHandler(IApplicationDbContext context, IIdentityServices identityServices, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<GetAllCommitteeMemberOfEventResponse> Handle(GetAllCommitteeMemberOfEventQuery query, CancellationToken cancellationToken)
    {
        var cachedValue = _cacheService.GetCacheValue<GetAllCommitteeMemberOfEventResponse>(
            InMemoryCacheKey.CommitteeMemberOfEvent(query.EventId));

        if (cachedValue != null)
        {
            return cachedValue;
        }

        var entities = await _context.CommitteeMembers
            .Where(x => x.EventId == query.EventId)
            .Include(x => x.User)
            .Include(x => x.CommitteeRole)
            .ToListAsync(cancellationToken);

        var roles = await _context.CommitteeRoles.ToListAsync(cancellationToken);

        var result = entities.ToCommitteeMemberResponse(roles);

        _cacheService.SetCacheValue(InMemoryCacheKey.CommitteeMemberOfEvent(query.EventId), result);

        return result;
    }
}