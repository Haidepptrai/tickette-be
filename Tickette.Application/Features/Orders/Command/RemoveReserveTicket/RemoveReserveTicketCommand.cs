using System.Text.Json;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common;

namespace Tickette.Application.Features.Orders.Command.RemoveReserveTicket;

public record RemoveReserveTicketCommand
{
    public Guid UserId { get; set; }
    public ICollection<TicketReservation> Tickets { get; init; }

    public void UpdateUserId(Guid userId)
    {
        UserId = userId;
    }
}

public class RemoveReserveTicketCommandHandler : ICommandHandler<RemoveReserveTicketCommand, Unit>
{
    private readonly IMessageProducer _messageProducer;

    public RemoveReserveTicketCommandHandler(IMessageProducer messageProducer)
    {
        _messageProducer = messageProducer;
    }

    public async Task<Unit> Handle(RemoveReserveTicketCommand query, CancellationToken cancellation)
    {
        var message = JsonSerializer.Serialize(query);

        var successSending = await _messageProducer.PublishAsync(RabbitMqRoutingKeys.TicketReservationCancelled, message);

        if (!successSending)
            throw new Exception("Failed to send message to the queue");

        return Unit.Value;
    }
}