using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tickette.Application.Common;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;

namespace Tickette.Application.Features.Events.Queries;

public record GetEventInMonth
{
    public int Month { get; init; } = DateTime.Now.Month;
}

public class GetEventInMonthHandler : BaseHandler<GetEventInMonthHandler>, IQueryHandler<GetEventInMonth, IEnumerable<EventListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEventInMonthHandler(IApplicationDbContext context, ILogger<GetEventInMonthHandler> logger) : base(logger)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventListDto>> Handle(GetEventInMonth query, CancellationToken cancellation)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var events = await _context.Events.Include(e => e.Committee)
                .Where(e => e.StartDate.Month == query.Month).ToListAsync(cancellation);

            var eventsDto = events.Select(e => e.ToEventListDto());

            return eventsDto;
        }, "Get Event In Month");
    }
}