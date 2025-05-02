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
    private readonly IMessageRequestClient _messageRequestClient;

    public RemoveReserveTicketCommandHandler(IMessageRequestClient messageRequestClient)
    {
        _messageRequestClient = messageRequestClient;
    }

    public async Task<Unit> Handle(RemoveReserveTicketCommand query, CancellationToken cancellation)
    {
        await _messageRequestClient.FireAndForgetAsync(query, cancellation);
        return Unit.Value;
    }
}