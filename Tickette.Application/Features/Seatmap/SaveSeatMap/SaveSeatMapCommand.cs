using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Domain.Common;

namespace Tickette.Application.Features.Seatmap.SaveSeatMap;

using Tickette.Application.Common.Interfaces;
using Tickette.Domain.Entities;

public record SaveSeatMapCommand
{
    public Guid EventDateId { get; init; }
    public ICollection<EventSeatMapSection> Shapes { get; init; } = new List<EventSeatMapSection>();
    public ICollection<TicketSeatMapping> Tickets { get; init; } = new List<TicketSeatMapping>();
}

public class SaveSeatMapCommandHandler : ICommandHandler<SaveSeatMapCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public SaveSeatMapCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(SaveSeatMapCommand request, CancellationToken cancellationToken)
    {
        var seatMap = EventSeatMap.CreateEventSeatMap(request.Shapes, request.Tickets);

        var eventDate = await _context.EventDates
            .Include(ed => ed.Event)
            .FirstOrDefaultAsync(ed => ed.Id == request.EventDateId, cancellationToken);

        if (eventDate is null)
        {
            throw new ApplicationException(nameof(EventDate));
        }

        eventDate.AddSeatMap(seatMap);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}