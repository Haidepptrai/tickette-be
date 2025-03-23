using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;
using Tickette.Application.Features.Events.Common;
using Tickette.Application.Features.Events.Common.Client;

namespace Tickette.Application.Features.Events.Queries.Client.GetEventDetailStatistic;

public record GetEventDetailStatisticQuery
{
    public Guid EventId { get; init; }
}

public class GetEventDetailStatisticQueryHandler : IQueryHandler<GetEventDetailStatisticQuery, EventDetailStatisticDto>
{
    private readonly IApplicationDbContext _context;

    public GetEventDetailStatisticQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventDetailStatisticDto> Handle(GetEventDetailStatisticQuery query, CancellationToken cancellation)
    {
        var eventDetails = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Committee)
            .Include(e => e.EventDates)
                .ThenInclude(ed => ed.Tickets)
            .Include(e => e.CommitteeMembers)
                .ThenInclude(cm => cm.User)
            .Include(e => e.CommitteeMembers)
                .ThenInclude(cm => cm.CommitteeRole)

            .FirstOrDefaultAsync(e => e.Id == query.EventId, cancellation);

        if (eventDetails == null)
        {
            throw new NotFoundException("Event", query.EventId);
        }

        return eventDetails.ToEventDetailStatisticDto();
    }
}
