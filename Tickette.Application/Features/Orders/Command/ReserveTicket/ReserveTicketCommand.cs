using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
    private IMemoryCache _memoryCache;

    public ReserveTicketCommandHandler(
        IApplicationDbContext context,
         IMessageRequestClient requestClient, IMemoryCache memoryCache)
    {
        _context = context;
        _requestClient = requestClient;
        _memoryCache = memoryCache;
    }

    public async Task<Unit> Handle(ReserveTicketCommand request, CancellationToken cancellationToken)
    {
        foreach (var ticket in request.Tickets)
        {
            if (_memoryCache.TryGetValue<Ticket>(ticket.Id, out var cachedTicket))
            {
                // Cached: validate directly
                cachedTicket?.ValidateTicket(ticket.Quantity);
                continue; // Skip DB hit
            }

            // Cache miss: hit the DB
            var ticketEntity = await _context.Tickets
                .SingleOrDefaultAsync(t => t.Id == ticket.Id, cancellationToken);

            if (ticketEntity is null)
            {
                throw new NotFoundException("Ticket", ticket.Id);
            }

            // Validate and then cache
            ticketEntity.ValidateTicket(ticket.Quantity);

            _memoryCache.Set(ticket.Id, ticketEntity, TimeSpan.FromMinutes(5));
        }

        // Wait for response from consumer
        var response = await _requestClient.PublishAsync<ReserveTicketCommand, RedisReservationResult>(
            request,
            cancellationToken);

        if (!response.Success)
            throw new TicketReservationException(response.Message ?? "Unknown error");

        return Unit.Value;
    }
}