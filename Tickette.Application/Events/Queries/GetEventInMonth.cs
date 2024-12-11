using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Events.Common;

namespace Tickette.Application.Events.Queries;

public record GetEventInMonth
{
    public int Month { get; init; } = DateTime.Now.Month;
}

public class GetEventInMonthHandler : IQueryHandler<GetEventInMonth, IEnumerable<EventListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEventInMonthHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventListDto>> Handle(GetEventInMonth query, CancellationToken cancellation)
    {
        var events = await _context.Events.Include(e => e.Committee)
            .Where(e => e.StartDate.Month == query.Month).ToListAsync(cancellation);


        var eventsDto = events.Select(e => e.ToEventListDto());
        return eventsDto;
    }
}