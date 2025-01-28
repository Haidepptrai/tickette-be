using System.Text.Json;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common;

namespace Tickette.Application.Features.Orders.Command.ReverseTicket;

public record ReverseTicketCommand
{
    public required Guid UserId { get; init; }
    public required ICollection<TicketReservation> Tickets { get; set; }
    public required DateTime ReverseTime { get; init; }
}

public class ReverseTicketCommandHandler : ICommandHandler<ReverseTicketCommand, Unit>
{
    private readonly IMessageProducer _messageProducer;

    public ReverseTicketCommandHandler(IMessageProducer messageProducer)
    {
        _messageProducer = messageProducer;
    }

    public Task<Unit> Handle(ReverseTicketCommand request, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Serialize(request);
        _messageProducer.Publish(Constant.TICKET_RESERVATION_QUEUE, message);
        return Task.FromResult(Unit.Value);
    }
}