using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tickette.Application.Common;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;

namespace Tickette.Application.Features.Events.Queries;

public record GetEventInNearbyWeekend
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public class GetEventInNearbyWeekendHandler : BaseHandler<GetEventInNearbyWeekendHandler>, IQueryHandler<GetEventInNearbyWeekend, IEnumerable<EventListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEventInNearbyWeekendHandler(IApplicationDbContext context, ILogger<GetEventInNearbyWeekendHandler> logger) : base(logger)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventListDto>> Handle(GetEventInNearbyWeekend query, CancellationToken cancellation)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var today = DateTime.UtcNow.Date;
            var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
            var saturday = today.AddDays(daysUntilSaturday);
            var sunday = saturday.AddDays(1);

            var events = await _context.Events
                .Where(e => e.StartDate.Date >= saturday && e.StartDate.Date <= sunday)
                .AsNoTracking()
                .ToListAsync(cancellation);

            var eventsDto = events.Select(e => e.ToEventListDto());
            return eventsDto;
        }, "Get Event In The Weekend");
    }
}