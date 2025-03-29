using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common;

namespace Tickette.Application.Features.Orders.Command.RemoveReserveTicket;

public record RemoveReserveTicketCommand
{
    public Guid UserId { get; set; }
    public ICollection<TicketReservation> Tickets { get; init; }
}

public class RemoveReserveTicketCommandHandler : ICommandHandler<RemoveReserveTicketCommand, Unit>
{
    private readonly IReservationService _reservationService;

    public RemoveReserveTicketCommandHandler(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public async Task<Unit> Handle(RemoveReserveTicketCommand query, CancellationToken cancellation)
    {
        foreach (var ticket in query.Tickets)
        {
            await _reservationService.ReleaseReservationAsync(query.UserId, ticket);
        }
        return Unit.Value;
    }
}