using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Exceptions;
using Tickette.Application.Features.Orders.Common;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;
using Tickette.Domain.Common.Exceptions;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Orders.Command.ReserveTicket;

public record ReserveTicketCommand
{
    public Guid UserId { get; set; }
    public Guid EventDateId { get; set; }
    public bool HasSeatMap { get; set; }
    public required ICollection<TicketReservation> Tickets { get; init; }

    public void UpdateUserId(Guid userId)
    {
        UserId = userId;
    }
}

public class ReserveTicketCommandHandler : ICommandHandler<ReserveTicketCommand, Unit>
{
    private readonly IMessageRequestClient _requestClient;
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _memoryCache;

    public ReserveTicketCommandHandler(
        IApplicationDbContext context,
         IMessageRequestClient requestClient, ICacheService memoryCache)
    {
        _context = context;
        _requestClient = requestClient;
        _memoryCache = memoryCache;
    }

    public async Task<Unit> Handle(ReserveTicketCommand request, CancellationToken cancellationToken)
    {
        EventDate? eventDate = null;

        // Load EventDate only if needed
        if (request.HasSeatMap)
        {
            eventDate = await _context.EventDates
                .SingleOrDefaultAsync(e => e.Id == request.EventDateId, cancellationToken);

            if (eventDate is null)
                throw new NotFoundException("EventDate", request.EventDateId);
        }

        foreach (var ticket in request.Tickets)
        {
            var cachedTicket = _memoryCache.GetCacheValue<Ticket>(ticket.Id.ToString());

            if (cachedTicket != null)
            {
                if (request.HasSeatMap)
                {
                    eventDate!.ValidateSelection(eventDate.SeatMap!, ticket.SeatsChosen!);
                }
                cachedTicket.ValidateTicket(ticket.Quantity);
            }
            else
            {
                var ticketEntity = await _context.Tickets
                    .Include(t => t.EventDate) // optional, only if needed later
                    .SingleOrDefaultAsync(t => t.Id == ticket.Id, cancellationToken);

                if (ticketEntity is null)
                    throw new NotFoundException("Ticket", ticket.Id);

                if (request.HasSeatMap)
                {
                    eventDate!.ValidateSelection(eventDate.SeatMap!, ticket.SeatsChosen!);
                }

                cachedTicket?.ValidateTicket(ticket.Quantity);

                ticketEntity.ValidateTicket(ticket.Quantity);

                _memoryCache.SetCacheValue(ticketEntity.Id.ToString(), TimeSpan.FromMinutes(15));
            }
        }

        var response = await _requestClient.PublishAsync<ReserveTicketCommand, RabbitMQMessageResult>(
            request, cancellationToken);

        if (!response.Success)
            throw new TicketReservationException(response.Message ?? "Unknown error");

        return Unit.Value;
    }
}