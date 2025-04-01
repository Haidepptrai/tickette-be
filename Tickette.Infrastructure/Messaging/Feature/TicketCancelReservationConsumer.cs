using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Tickette.Application.Common.Constants;
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
            var command = JsonSerializer.Deserialize<RemoveReserveTicketCommand>(message);

            if (command == null)
                throw new InvalidOperationException("Failed to deserialize RemoveReserveTicketCommand");

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IReservationService>();

            foreach (var ticket in command.Tickets)
            {
                await handler.ReleaseReservationAsync(command.UserId, ticket);
            }
        }, stoppingToken);
    }
}