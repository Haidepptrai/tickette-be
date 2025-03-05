using System.Text.Json;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common;
using Tickette.Infrastructure.Helpers;

namespace Tickette.Application.Features.Orders.Command.ReserveTicket;

public record ReserveTicketCommand
{
    public required Guid UserId { get; init; }
    public required ICollection<TicketReservation> Tickets { get; set; }
}

public class ReserveTicketCommandHandler : ICommandHandler<ReserveTicketCommand, Unit>
{
    private readonly IMessageProducer _messageProducer;
    private readonly IRedisService _redisService;

    public ReserveTicketCommandHandler(IMessageProducer messageProducer, IRedisService redisService, IApplicationDbContext context)
    {
        _messageProducer = messageProducer;
        _redisService = redisService;
    }

    public async Task<Unit> Handle(ReserveTicketCommand request, CancellationToken cancellationToken)
    {
        foreach (var ticket in request.Tickets)
        {

            string inventoryKey = RedisKeys.GetTicketQuantityKey(ticket.Id);

            // Atomic decrement in Redis to prevent overselling
            long remainingTickets = await _redisService.DecrementAsync(inventoryKey, ticket.Quantity);

            if (remainingTickets < 0)
            {
                // Rollback if oversold
                await _redisService.IncrementAsync(inventoryKey, ticket.Quantity);
                throw new Exception($"Not enough tickets available for Ticket {ticket.Id}");
            }

            // Store only the quantity locked by the user (not the entire command)
            string reservationKey = RedisKeys.GetReservationKey(ticket.Id, request.UserId);

            var reservationData = new
            {
                UserId = request.UserId,
                Quantity = ticket.Quantity,
                ReservedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            await _redisService.SetAsync(reservationKey, JsonSerializer.Serialize(reservationData), 15);
        }

        // Publish to RabbitMQ for PostgreSQL update
        var message = JsonSerializer.Serialize(request);
        _messageProducer.Publish(Constant.TICKET_RESERVATION_QUEUE, message);

        return Unit.Value;
    }
}