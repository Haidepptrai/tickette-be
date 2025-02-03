using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common;

namespace Tickette.Application.Features.Orders.Command.RemoveReserveTicket;

public record RemoveReserveTicketCommand
{
    public Guid UserId { get; init; }
    public ICollection<TicketReservation> Tickets { get; init; }
}

public class RemoveReserveTicketCommandHandler : ICommandHandler<RemoveReserveTicketCommand, Unit>
{
    private readonly IRedisService _redisService;

    public RemoveReserveTicketCommandHandler(IRedisService redisService)
    {
        _redisService = redisService;
    }

    public async Task<Unit> Handle(RemoveReserveTicketCommand command, CancellationToken cancellation)
    {
        foreach (var ticket in command.Tickets)
        {
            string reservationKey = $"reservation:{ticket.Id}:{command.UserId}";
            var reservationData = await _redisService.GetAsync(reservationKey);

            if (reservationData == null)
            {
                return Unit.Value;
            }

            // Remove the reservation from Redis
            await _redisService.DeleteKeyAsync(reservationKey);

        }

        return Unit.Value;
    }
}