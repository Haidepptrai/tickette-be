using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common.Admin;
using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Events.Queries.Admin.GetEventsStatistic;

public record GetEventsStatisticQuery { }

public class GetEventsStatisticQueryHandler : IQueryHandler<GetEventsStatisticQuery, EventsStatisticDto>
{
    private readonly IApplicationDbContext _context;

    public GetEventsStatisticQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventsStatisticDto> Handle(GetEventsStatisticQuery query, CancellationToken cancellation)
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

        var pendingEvents = await _context.Events
            .CountAsync(e => e.Status == ApprovalStatus.Pending &&
                             e.StartDate >= firstDayOfMonth &&
                             e.StartDate < firstDayOfNextMonth, cancellation);

        var approvedEvents = await _context.Events
            .CountAsync(e => e.Status == ApprovalStatus.Approved &&
                             e.StartDate >= firstDayOfMonth &&
                             e.StartDate < firstDayOfNextMonth, cancellation);

        var rejectedEvents = await _context.Events
            .CountAsync(e => e.Status == ApprovalStatus.Rejected &&
                             e.StartDate >= firstDayOfMonth &&
                             e.StartDate < firstDayOfNextMonth, cancellation);

        var upcomingEvents = await _context.Events
            .CountAsync(e => e.StartDate >= firstDayOfMonth &&
                             e.StartDate < firstDayOfNextMonth, cancellation);

        return new EventsStatisticDto
        {
            PendingEvents = pendingEvents,
            ApprovedEvents = approvedEvents,
            RejectedEvents = rejectedEvents,
            UpComingEvents = upcomingEvents
        };
    }
}