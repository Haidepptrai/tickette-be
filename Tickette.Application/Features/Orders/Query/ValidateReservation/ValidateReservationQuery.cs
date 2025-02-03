using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;

namespace Tickette.Application.Features.Orders.Query.ValidateReservation;

public record ValidateReservationQuery
{
    public ICollection<TicketReservation> Tickets { get; init; }

    public Guid UserId { get; init; }
}

public class ValidateReservationQueryHandler : IQueryHandler<ValidateReservationQuery, bool>
{
    private readonly IRedisService _redisService;

    public ValidateReservationQueryHandler(IRedisService redisService)
    {
        _redisService = redisService;
    }

    public async Task<bool> Handle(ValidateReservationQuery query, CancellationToken cancellation)
    {
        foreach (var ticket in query.Tickets)
        {
            string reservationKey = $"reservation:{ticket.Id}:{query.UserId}";
            var exists = await _redisService.KeyExistsAsync(reservationKey);

            // No reservation found
            if (!exists)
            {
                // Increase the tickets quantity back
                string inventoryKey = $"ticket:{ticket.Id}:remaining_tickets";
                await _redisService.IncrementAsync(inventoryKey, ticket.Quantity);

                return false;
            }
        }
        return true;
    }
}