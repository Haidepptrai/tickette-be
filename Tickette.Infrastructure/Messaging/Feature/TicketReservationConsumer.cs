using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TicketReservationConsumer> _logger;

    public TicketReservationConsumer(
        IMessageConsumer messageConsumer,
        IRedisService redisService,
        IServiceProvider serviceProvider,
        ILogger<TicketReservationConsumer> logger)
    {
        _messageConsumer = messageConsumer;
        _redisService = redisService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Ticket Reservation Consumer is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _messageConsumer.ConsumeAsync(Constant.TICKET_RESERVATION_QUEUE, async (message) =>
                {
                    // Create a scope to resolve scoped services
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                        await HandleTicketReservation(message, context, stoppingToken);
                    }
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while consuming messages.");
            }

            await Task.Delay(1000, stoppingToken); // Add a delay to avoid tight looping
        }

        _logger.LogInformation("Ticket Reservation Consumer is stopping.");
    }

    private async Task HandleTicketReservation(string message, IApplicationDbContext context, CancellationToken cancellationToken)
    {
        var ticketReservation = JsonSerializer.Deserialize<ReserveTicketCommand>(message);
        if (ticketReservation == null)
        {
            _logger.LogError("Failed to deserialize reservation request.");
            throw new Exception("Failed to deserialize reservation request");
        }

        try
        {
            foreach (var ticket in ticketReservation.Tickets)
            {
                string reservationKey = $"reservation:{ticket.Id}:{ticketReservation.UserId}";

                // Verify reservation still exists in Redis
                bool exists = await _redisService.KeyExistsAsync(reservationKey);
                if (!exists) return;

                // Update PostgreSQL inventory per ticket
                var ticketRecord = await context.Tickets
                    .Where(t => t.Id == ticket.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ticketRecord == null || ticketRecord.RemainingTickets < ticket.Quantity)
                {
                    _logger.LogWarning($"Not enough tickets available for Ticket {ticket.Id}");
                    throw new Exception($"Not enough tickets available for Ticket {ticket.Id}");
                }

                ticketRecord.ReduceTickets(ticket.Quantity);
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing ticket reservation: {ex.Message}");
            throw;
        }
    }
}