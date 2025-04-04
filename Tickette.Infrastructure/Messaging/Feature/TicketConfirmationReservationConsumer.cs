using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Command.CreateOrder;
using Tickette.Infrastructure.Services;

namespace Tickette.Infrastructure.Messaging.Feature;

public class TicketConfirmationReservationConsumer : BackgroundService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly IServiceProvider _serviceProvider;

    public TicketConfirmationReservationConsumer(IMessageConsumer messageConsumer, IServiceProvider serviceProvider)
    {
        _messageConsumer = messageConsumer;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _messageConsumer.ConsumeAsync(RabbitMqRoutingKeys.TicketReservationConfirmed, async (message) =>
        {
            try
            {
                var command = JsonSerializer.Deserialize<CreateOrderCommand>(message);

                if (command == null)
                {
                    Console.WriteLine("[Warning] Failed to deserialize RemoveReserveTicketCommand");
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var redisHandler = scope.ServiceProvider.GetRequiredService<IReservationService>();
                var dbHandler = scope.ServiceProvider.GetRequiredService<ReservationPersistenceService>();

                foreach (var ticket in command.Tickets)
                {
                    // 1. Remove from Redis
                    var redisReleased = await redisHandler.FinalizeSeatReservationAsync(command.UserId, ticket);
                    if (!redisReleased)
                    {
                        Console.WriteLine($"[Warning] Could not release Redis for ticket {ticket.Id}");
                    }

                    // 2. Mark reservation as confirmed in DB
                    var dbReleased = await dbHandler.ConfirmReservationInDatabaseAsync(command.UserId, ticket.Id);
                    if (!dbReleased)
                    {
                        Console.WriteLine($"[Warning] Could not find DB reservation to cancel for ticket {ticket.Id}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Fatal] Error in TicketReservationCancelled consumer: {ex.Message}");
                // Log and exit gracefully — don't rethrow
            }

        }, stoppingToken);
    }
}