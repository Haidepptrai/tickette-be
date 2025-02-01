using System.Text.Json;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common;

namespace Tickette.Application.Features.Orders.Command.ReserveTicket;

public record ReserveTicketCommand
{
    public required Guid UserId { get; init; }
    public required Guid EventId { get; init; }
    public required ICollection<TicketReservation> Tickets { get; set; }
    public required DateTime ReverseTime { get; init; } = DateTime.UtcNow;
}

public class ReserveTicketCommandHandler : ICommandHandler<ReserveTicketCommand, Unit>
{
    private readonly IMessageProducer _messageProducer;
    private readonly IRedisService _redisService;
    private readonly IApplicationDbContext _context;

    public ReserveTicketCommandHandler(IMessageProducer messageProducer, IRedisService redisService, IApplicationDbContext context)
    {
        _messageProducer = messageProducer;
        _redisService = redisService;
        _context = context;
    }

    public async Task<Unit> Handle(ReserveTicketCommand request, CancellationToken cancellationToken)
    {
        foreach (var ticket in request.Tickets)
        {
            string inventoryKey = $"event:{request.EventId}:ticket:{ticket.TicketId}:remaining_tickets";

            // Atomic decrement in Redis to prevent overselling
            long remainingTickets = await _redisService.DecrementAsync(inventoryKey, ticket.Quantity);

            if (remainingTickets < 0)
            {
                // Rollback if oversold
                await _redisService.IncrementAsync(inventoryKey, ticket.Quantity);
                throw new Exception($"Not enough tickets available for Ticket {ticket.TicketId}");
            }

            // Store only the quantity locked by the user (not the entire command)
            string reservationKey = $"reservation:{ticket.TicketId}:{request.UserId}";

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

        // This is just reserve ticket for user
        // The actual order will be created in the OrderTicketsCommandHandler
        return Unit.Value;
    }
}