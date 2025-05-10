using System.Text.Json.Serialization;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;

namespace Tickette.Application.Features.Orders.Query.CheckReservationValidation;

public record CheckReservationValidationQuery
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public required ICollection<TicketReservation> SelectedTickets { get; init; }
}

public class CheckReservationValidationQueryHandler : IQueryHandler<CheckReservationValidationQuery, bool>
{
    private readonly IReservationService _reservationService;

    public CheckReservationValidationQueryHandler(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public async Task<bool> Handle(CheckReservationValidationQuery request, CancellationToken cancellationToken)
    {
        return await _reservationService.ValidateReservationAsync(request.UserId, request.SelectedTickets.First());
    }
}