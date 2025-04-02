using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common;
using Tickette.Domain.Common.Exceptions;

namespace Tickette.Application.Features.Orders.Query.ValidateReservation;

public record ValidateReservationQuery
{
    public ICollection<TicketReservation> Tickets { get; init; }

    public Guid UserId { get; init; }
}

public class ValidateReservationQueryHandler : IQueryHandler<ValidateReservationQuery, Unit>
{

    private readonly IReservationService _reservationService;

    public ValidateReservationQueryHandler(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public async Task<Unit> Handle(ValidateReservationQuery query, CancellationToken cancellation)
    {
        foreach (var ticket in query.Tickets)
        {
            var valid = await _reservationService.ValidateReservationAsync(query.UserId, ticket);

            if (!valid)
            {
                throw new InvalidReservationException();
            }
        }

        return Unit.Value;
    }
}