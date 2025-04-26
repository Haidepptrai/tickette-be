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
using Tickette.Infrastructure.Messaging;

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

        // Wait for response from consumer
        var result = await _messageProducer.PublishAsync(
            RabbitMqRoutingKeys.TicketReservationCreated,
            message,
            TimeSpan.FromSeconds(5)
        );

        if (result is null)
            throw new TicketReservationException("Timed out waiting for reservation confirmation.");

        var response = JsonSerializer.Deserialize<RedisReservationResult>(result);

        if (response is null || !response.Success)
            throw new TicketReservationException(response?.Message ?? "Unknown error");

        return Unit.Value;
    }
}