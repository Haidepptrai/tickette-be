using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Seatmap.Query;

public record GetEventDateSeatMapQuery
{
    public Guid EventDateId { get; init; }
}

public class GetEventDateSeatMapQueryHandler : IQueryHandler<GetEventDateSeatMapQuery, EventSeatMap?>
{
    public readonly IApplicationDbContext _context;

    public GetEventDateSeatMapQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventSeatMap?> Handle(GetEventDateSeatMapQuery query, CancellationToken cancellation)
    {
        var eventDate = await _context.EventDates
            .Where(ed => ed.Id == query.EventDateId)
            .SingleOrDefaultAsync(cancellation);

        if (eventDate == null)
        {
            throw new NotFoundException("Event Date", query.EventDateId);
        }

        var seatMap = eventDate.SeatMap;

        return seatMap;
    }
}