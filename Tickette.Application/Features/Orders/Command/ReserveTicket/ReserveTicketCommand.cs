using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Exceptions;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common;

namespace Tickette.Application.Features.Orders.Command.ReserveTicket;

public record ReserveTicketCommand
{
    public Guid UserId { get; set; }
    public required ICollection<TicketReservation> Tickets { get; init; }

    public void UpdateUserId(Guid userId)
    {
        UserId = userId;
    }
}

public class ReserveTicketCommandHandler : ICommandHandler<ReserveTicketCommand, Unit>
{
    private readonly IMessageProducer _messageProducer;
    private readonly IApplicationDbContext _context;
    private readonly IReservationService _reservationService;

    public ReserveTicketCommandHandler(IApplicationDbContext context, IReservationService reservationService, IMessageProducer messageProducer)
    {
        _context = context;
        _reservationService = reservationService;
        _messageProducer = messageProducer;
    }

    public async Task<Unit> Handle(ReserveTicketCommand request, CancellationToken cancellationToken)
    {
        foreach (var ticket in request.Tickets)
        {
            // Is the ticket valid?
            var ticketEntity = await _context.Tickets.SingleOrDefaultAsync(t => t.Id == ticket.Id, cancellationToken);
            if (ticketEntity == null)
            {
                throw new NotFoundException("Ticket", ticket.Id);
            }

            ticketEntity.ValidateTicket(ticket.Quantity);
        }

        var message = JsonSerializer.Serialize(request);
        var successSending = await _messageProducer.PublishAsync(RabbitMqRoutingKeys.TicketReservationCreated, message);

        if (!successSending)
        {
            throw new Exception("Failed to send message to the queue");
        }

        return Unit.Value;
    }
}