using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Command.ReserveTicket;
using Tickette.Domain.Common;

namespace Tickette.Infrastructure.Messaging.Feature;

public class TicketReservationConsumer : BackgroundService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly IRedisService _redisService;
    private readonly IApplicationDbContext _context;

    public TicketReservationConsumer(IMessageConsumer messageConsumer, IRedisService redisService, IApplicationDbContext context)
    {
        _messageConsumer = messageConsumer;
        _redisService = redisService;
        _context = context;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _messageConsumer.Consume(Constant.TICKET_RESERVATION_QUEUE, async void (message) =>
        {
            try
            {
                await HandleTicketReservation(message, stoppingToken);
            }
            catch (Exception e)
            {
                throw new Exception($"Error processing message: {e.Message}");
            }
        });

        await Task.CompletedTask;
    }

    private async Task HandleTicketReservation(string message, CancellationToken cancellationToken)
    {
        var ticketReservation = JsonSerializer.Deserialize<ReserveTicketCommand>(message);
        if (ticketReservation == null) throw new Exception("Failed to deserialize reservation request");

        try
        {
            foreach (var ticket in ticketReservation.Tickets)
            {
                string reservationKey = $"reservation:{ticket.TicketId}:{ticketReservation.UserId}";

                // Verify reservation still exists in Redis
                bool exists = await _redisService.KeyExistsAsync(reservationKey);
                if (!exists)
                {
                    throw new Exception($"Reservation expired or not found in Redis for Ticket {ticket.TicketId}");
                }

                // Update PostgreSQL inventory per ticket
                var ticketRecord = await _context.Tickets
                    .Where(t => t.Id == ticket.TicketId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ticketRecord == null || ticketRecord.RemainingTickets < ticket.Quantity)
                {
                    throw new Exception($"Not enough tickets available for Ticket {ticket.TicketId}");
                }

                ticketRecord.ReduceTickets(ticket.Quantity);
                await _context.SaveChangesAsync(cancellationToken);

                // Remove reservation from Redis after confirming in DB
                await _redisService.DeleteKeyAsync(reservationKey);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing ticket reservation: {ex.Message}");
            throw;
        }
    }

}