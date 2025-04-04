using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Command.ReserveTicket;
using Tickette.Domain.Common.Exceptions;
using Tickette.Infrastructure.Services;

namespace Tickette.Infrastructure.Messaging.Feature;

public class TicketReservationConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageConsumer _messageConsumer;

    public TicketReservationConsumer(IServiceProvider serviceProvider, IMessageConsumer messageConsumer)
    {
        _serviceProvider = serviceProvider;
        _messageConsumer = messageConsumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _messageConsumer.ConsumeAsync(RabbitMqRoutingKeys.TicketReservationCreated, async (message) =>
        {
            var reservation = JsonSerializer.Deserialize<ReserveTicketCommand>(message);

            if (reservation == null)
            {
                throw new InvalidOperationException("Failed to deserialize ReserveTicketCommand");
            }

            using var scope = _serviceProvider.CreateScope();

            var redisHandler = scope.ServiceProvider.GetRequiredService<IReservationService>();
            var persistenceService = scope.ServiceProvider.GetRequiredService<ReservationPersistenceService>();

            foreach (var ticket in reservation.Tickets)
            {
                try
                {
                    await persistenceService.PersistReservationAsync(reservation.UserId, ticket);
                }
                catch (Exception ex)
                {
                    await redisHandler.ReleaseReservationAsync(reservation.UserId, ticket);
                    Console.WriteLine($"[Rollback Error] Failed to release Redis for ticket {ticket.Id}: {ex.Message}");
                    throw new TicketReservationException($"Failed to reserved for ticket {ticket.Id}, please try again");
                }
            }
        }, stoppingToken);
    }
}