using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;
using Tickette.Application.Wrappers;
using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Events.Queries.Client.GetEventByUserId;

public record GetEventByUserIdQuery
{
    [JsonIgnore]
    public Guid UserId { get; set; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public ApprovalStatus? Status { get; init; }
}

public class GetEventByUserIdQueryHandler : IQueryHandler<GetEventByUserIdQuery, PagedResult<UserEventListResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetEventByUserIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<UserEventListResponse>> Handle(GetEventByUserIdQuery query, CancellationToken cancellationToken)
    {
        var preparedQuery = _context.CommitteeMembers
            .IgnoreQueryFilters()
            .Where(cm => cm.UserId == query.UserId)
            .Include(cm => cm.Event).ThenInclude(e => e.Category)
            .Include(cm => cm.Event).ThenInclude(e => e.Committee)
            .Include(cm => cm.Event).ThenInclude(e => e.EventDates)
            .Where(cm => !query.Status.HasValue || cm.Event.Status == query.Status.Value)
            .Select(cm => cm.Event);

        var events = await preparedQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var count = await preparedQuery.CountAsync(cancellationToken);

        var eventsToListDto = events.Select(e => e.ToUserEventListResponse()).ToList();

        var result = new PagedResult<UserEventListResponse>(eventsToListDto, count, query.PageSize, query.PageSize);

        return result;
    }
}