using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Command.ReserveTicket;
using Tickette.Domain.Common.Exceptions;

namespace Tickette.Infrastructure.Messaging.Feature;

/// <summary>
/// This class is responsible for synchronizing the ticket reservation with database
/// after done in Redis.
/// Since user need to know if the reservation is done or not, so we cannot
/// put redis cache reservation in background.
/// </summary>
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
                return "InvalidRequest";

            using var scope = _serviceProvider.CreateScope();

            var redisHandler = scope.ServiceProvider.GetRequiredService<IReservationService>();
            var persistenceService = scope.ServiceProvider.GetRequiredService<IReservationDbSyncService>();

            foreach (var ticket in reservation.Tickets)
            {
                try
                {
                    await redisHandler.ReserveTicketsAsync(reservation.UserId, ticket);
                    await persistenceService.PersistReservationAsync(reservation.UserId, ticket);
                }
                catch (SeatOrderedException ex)
                {
                    await redisHandler.ReleaseReservationAsync(reservation.UserId, ticket);
                    return JsonSerializer.Serialize(RedisReservationResult.Fail(ex.Message, "SeatConflict"));
                }
                catch (TicketReservationException ex)
                {
                    await redisHandler.ReleaseReservationAsync(reservation.UserId, ticket);
                    return JsonSerializer.Serialize(RedisReservationResult.Fail(ex.Message, "InventoryIssue"));
                }
                catch (Exception ex)
                {
                    await redisHandler.ReleaseReservationAsync(reservation.UserId, ticket);
                    return JsonSerializer.Serialize(RedisReservationResult.Fail("Unexpected error occurred", "UnhandledException"));
                }
            }

            return JsonSerializer.Serialize(RedisReservationResult.Ok());
        }, stoppingToken);
    }
}