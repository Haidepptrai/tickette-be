using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common.Client;
using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Events.Queries.Client.GetSeatsOrderedInfo;

public record GetSeatsOrderedInfoQuery
{
    public ICollection<Guid> TicketIds { get; init; }
}

public class GetSeatsOrderedInfoQueryHandler : IQueryHandler<GetSeatsOrderedInfoQuery, IEnumerable<SeatsOrderedInfoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSeatsOrderedInfoQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SeatsOrderedInfoDto>> Handle(GetSeatsOrderedInfoQuery request, CancellationToken cancellationToken)
    {
        var seatsOrderedInfo = new List<SeatsOrderedInfoDto>();
        foreach (var ticketId in request.TicketIds)
        {
            var reservations = await _context.ReservationItems
                .Include(ri => ri.SeatAssignments)
                .Include(ri => ri.Reservation)
                .Where(ri => ri.TicketId == ticketId && ri.Reservation.Status == ReservationStatus.Confirmed)
                .ToListAsync(cancellationToken);

            foreach (var reservation in reservations)
            {
                foreach (var seat in reservation.SeatAssignments)
                {
                    seatsOrderedInfo.Add(new SeatsOrderedInfoDto
                    {
                        RowName = seat.RowName,
                        SeatNumber = seat.SeatNumber
                    });
                }
            }

        }

        return seatsOrderedInfo;
    }
}
