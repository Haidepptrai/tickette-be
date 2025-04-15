using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.EventDates.Common;

namespace Tickette.Application.Features.EventDates.Query;

public record GetEventDatesQuery
{
    public Guid EventId { get; init; }
}

public class GetEventDatesQueryHandler : IQueryHandler<GetEventDatesQuery, IEnumerable<EventDateForSeatMapDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEventDatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventDateForSeatMapDto>> Handle(GetEventDatesQuery request, CancellationToken cancellationToken)
    {
        var eventDate = await _context.EventDates
            .Include(ed => ed.Event)
            .Include(ed => ed.Tickets)
            .Where(ed => ed.EventId == request.EventId)
            .Select(ed => ed.MapEventDateToDto())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return eventDate;
    }
}