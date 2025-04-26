using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Command.RemoveReserveTicket;

namespace Tickette.Infrastructure.Messaging.Feature;

public class TicketCancelReservationConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageConsumer _messageConsumer;

    public TicketCancelReservationConsumer(IServiceProvider serviceProvider, IMessageConsumer messageConsumer)
    {
        _serviceProvider = serviceProvider;
        _messageConsumer = messageConsumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _messageConsumer.ConsumeAsync(RabbitMqRoutingKeys.TicketReservationCancelled, async (message) =>
        {
            try
            {
                var command = JsonSerializer.Deserialize<RemoveReserveTicketCommand>(message);

                if (command == null)
                {
                    Console.WriteLine("[Warning] Failed to deserialize RemoveReserveTicketCommand");
                    return null;
                }

                using var scope = _serviceProvider.CreateScope();
                var redisHandler = scope.ServiceProvider.GetRequiredService<IReservationService>();
                var dbSyncHandler = scope.ServiceProvider.GetRequiredService<IReservationDbSyncService>();

                foreach (var ticket in command.Tickets)
                {
                    try
                    {
                        // 1. Remove from Redis (temp reservation)
                        var redisReleased = await redisHandler.ReleaseReservationAsync(command.UserId, ticket);
                        if (!redisReleased)
                        {
                            Console.WriteLine($"[Warning] Could not release Redis for ticket {ticket.Id}");
                        }

                        // 2. Remove from DB (permanent reservation)
                        var dbReleased = await dbSyncHandler.ReleaseReservationFromDatabaseAsync(command.UserId, ticket.Id, false);
                        if (!dbReleased)
                        {
                            Console.WriteLine($"[Warning] Could not release DB for ticket {ticket.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Error] Failed to cancel ticket {ticket.Id}: {ex.Message}");
                        // Swallow and continue — don't stop consumer
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Fatal] Error in TicketReservationCancelled consumer: {ex.Message}");
                // Log and exit gracefully — don't rethrow
            }
            return null;
        }, stoppingToken);
    }

}