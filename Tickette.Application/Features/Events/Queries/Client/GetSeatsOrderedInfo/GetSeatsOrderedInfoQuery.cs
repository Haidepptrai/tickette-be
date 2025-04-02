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
            var reservationItem = await _context.ReservationItems
                .Include(x => x.SeatAssignments)
                .Include(x => x.Reservation)
                .Where(x => x.TicketId == ticketId && x.Reservation.Status == ReservationStatus.Confirmed)
                .FirstOrDefaultAsync(cancellationToken);

            if (reservationItem == null)
            {
                continue;
            }

            foreach (var seatAssignment in reservationItem.SeatAssignments)
            {
                seatsOrderedInfo.Add(new SeatsOrderedInfoDto
                {
                    RowName = seatAssignment.RowName,
                    SeatNumber = seatAssignment.SeatNumber
                });
            }

        }

        return seatsOrderedInfo;
    }
}
