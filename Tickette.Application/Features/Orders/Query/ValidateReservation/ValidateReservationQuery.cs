using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Exceptions;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common;
using Tickette.Infrastructure.Helpers;

namespace Tickette.Application.Features.Orders.Query.ValidateReservation;

public record ValidateReservationQuery
{
    public ICollection<TicketReservation> Tickets { get; init; }

    public Guid UserId { get; init; }
}

public class ValidateReservationQueryHandler : IQueryHandler<ValidateReservationQuery, Unit>
{
    private readonly IRedisService _redisService;

    public ValidateReservationQueryHandler(IRedisService redisService)
    {
        _redisService = redisService;
    }

    public async Task<Unit> Handle(ValidateReservationQuery query, CancellationToken cancellation)
    {
        foreach (var ticket in query.Tickets)
        {
            string reservationKey = RedisKeys.GetReservationKey(ticket.Id, query.UserId);
            var exists = await _redisService.KeyExistsAsync(reservationKey);

            // No reservation found
            if (!exists)
            {
                // Increase the tickets quantity back
                string inventoryKey = RedisKeys.GetTicketQuantityKey(ticket.Id);
                await _redisService.IncrementAsync(inventoryKey, ticket.Quantity);

                throw new NotFoundTicketReservationException();
            }
        }

        return Unit.Value;
    }
}