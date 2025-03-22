using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Exceptions;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common;
using Tickette.Domain.Common.Exceptions;

namespace Tickette.Application.Features.Orders.Command.ReserveTicket;

public record ReserveTicketCommand
{
    public required Guid UserId { get; init; }
    public required ICollection<TicketReservation> Tickets { get; set; }
}

public class ReserveTicketCommandHandler : ICommandHandler<ReserveTicketCommand, Unit>
{
    private readonly IMessageProducer _messageProducer;
    private readonly IApplicationDbContext _context;
    private readonly IReservationService _reservationService;

    public ReserveTicketCommandHandler(IMessageProducer messageProducer, IApplicationDbContext context, IReservationService reservationService)
    {
        _messageProducer = messageProducer;
        _context = context;
        _reservationService = reservationService;
    }

    public async Task<Unit> Handle(ReserveTicketCommand request, CancellationToken cancellationToken)
    {
        foreach (var ticket in request.Tickets)
        {
            // Is the ticket valid?
            var ticketEntity = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticket.Id, cancellationToken);
            if (ticketEntity == null)
            {
                throw new NotFoundException("Ticket", ticket.Id);
            }

            ticketEntity.ValidateTicket(ticket.Quantity);
            var reservedTicket = await _reservationService.ReserveTicketsAsync(request.UserId, ticket);

            if (!reservedTicket)
            {
                throw new TicketReservationException("Ticket reservation failed");
            }
        }

        // Publish to RabbitMQ for PostgreSQL update
        var message = JsonSerializer.Serialize(request);
        _messageProducer.Publish(Constant.TICKET_RESERVATION_QUEUE, message);

        return Unit.Value;
    }
}