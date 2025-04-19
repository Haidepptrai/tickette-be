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
using Tickette.Domain.Common.Exceptions;

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
    private readonly IReservationDbSyncService _reservationDbSyncService;

    public ReserveTicketCommandHandler(IApplicationDbContext context, IReservationService reservationService, IMessageProducer messageProducer, IReservationDbSyncService reservationDbSyncService)
    {
        _context = context;
        _reservationService = reservationService;
        _messageProducer = messageProducer;
        _reservationDbSyncService = reservationDbSyncService;
    }

    public async Task<Unit> Handle(ReserveTicketCommand request, CancellationToken cancellationToken)
    {
        bool isSuccess = false;

        foreach (var ticket in request.Tickets)
        {
            // Is the ticket valid?
            var ticketEntity = await _context.Tickets.SingleOrDefaultAsync(t => t.Id == ticket.Id, cancellationToken);
            if (ticketEntity == null)
            {
                throw new NotFoundException("Ticket", ticket.Id);
            }

            ticketEntity.ValidateTicket(ticket.Quantity);

            // Check if the ticket is already reserved
            isSuccess = await _reservationService.ReserveTicketsAsync(request.UserId, ticket);
            // Sometimes Redis might fail to connect, so that if it fails, then we can add fallback
            // by reserving the ticket in the database
            if (!isSuccess)
            {
                var reserveToDatabase = await _reservationDbSyncService.PersistReservationAsync(request.UserId, ticket);
                if (!reserveToDatabase)
                {
                    throw new TicketReservationException("Failed to reserve tickets. Please try again");
                }

            }
        }

        // Only sync with database if Redis reservation was successful
        if (isSuccess)
        {
            var message = JsonSerializer.Serialize(request);
            await _messageProducer.PublishAsync(RabbitMqRoutingKeys.TicketReservationCreated, message);
        }
        else
        {
            throw new TicketReservationException("Failed to reserve tickets. Please try again");
        }

        return Unit.Value;
    }
}