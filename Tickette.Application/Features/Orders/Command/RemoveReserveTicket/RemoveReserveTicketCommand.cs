using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common;
using Tickette.Infrastructure.Helpers;

namespace Tickette.Application.Features.Orders.Command.RemoveReserveTicket;

public record RemoveReserveTicketCommand
{
    public Guid UserId { get; init; }
    public ICollection<TicketReservation> Tickets { get; init; }
}

public class RemoveReserveTicketCommandHandler : ICommandHandler<RemoveReserveTicketCommand, Unit>
{
    private readonly IRedisService _redisService;
    private readonly IApplicationDbContext _context;

    public RemoveReserveTicketCommandHandler(IRedisService redisService, IApplicationDbContext context)
    {
        _redisService = redisService;
        _context = context;
    }

    public async Task<Unit> Handle(RemoveReserveTicketCommand command, CancellationToken cancellation)
    {
        foreach (var ticket in command.Tickets)
        {
            string reservationKey = RedisKeys.GetReservationKey(ticket.Id, command.UserId);
            var exists = await _redisService.KeyExistsAsync(reservationKey);

            // No reservation found
            if (!exists)
            {
                // Increase the tickets quantity back
                string inventoryKey = RedisKeys.GetTicketQuantityKey(ticket.Id);

                // Update the ticket quantity in database
                var ticketRecord = await _context.Tickets
                    .Where(t => t.Id == ticket.Id)
                    .FirstOrDefaultAsync(cancellation);


                if (ticketRecord == null)
                {
                    continue;
                }

                // Increment the ticket quantity
                ticketRecord.IncreaseTickets(ticket.Quantity);

                await _redisService.IncrementAsync(inventoryKey, ticket.Quantity);

                continue;
            }

            // Remove the reservation from Redis
            await _redisService.DeleteKeyAsync(reservationKey);

        }

        return Unit.Value;
    }
}