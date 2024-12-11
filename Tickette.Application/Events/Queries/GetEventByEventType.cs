using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Events.Common;
using Tickette.Domain.Enums;

namespace Tickette.Application.Events.Queries;

public record GetEventByEventType
{
    public EventType? Type { get; init; }
}

public class GetEventByEventTypeQueryHandler : IQueryHandler<GetEventByEventType, IEnumerable<EventListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEventByEventTypeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventListDto>> Handle(GetEventByEventType query, CancellationToken cancellation)
    {
        var events = await _context.Events.Include(e => e.Committee)
            .Where(e => e.Type == query.Type).ToListAsync(cancellation);

        var eventList = events.Select(e => e.ToEventListDto()).ToList();

        return eventList;
    }
}