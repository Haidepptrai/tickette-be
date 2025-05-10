using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Command.RemoveReserveTicket;

namespace Tickette.Infrastructure.Messaging.Feature;

public class RemoveTicketReservationConsumer : IConsumer<RemoveReserveTicketCommand>
{
    private readonly IServiceProvider _serviceProvider;

    public RemoveTicketReservationConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Consume(ConsumeContext<RemoveReserveTicketCommand> context)
    {
        try
        {
            var request = context.Message;

            using var scope = _serviceProvider.CreateScope();
            var redisHandler = scope.ServiceProvider.GetRequiredService<IReservationService>();
            var dbSyncHandler = scope.ServiceProvider.GetRequiredService<IReservationDbSyncService>();

            // 1. Remove from Redis (temp reservation)
            await redisHandler.ReleaseReservationAsync(request.UserId, request.Tickets);

            // 2. Remove from DB (permanent reservation)
            //await dbSyncHandler.ReleaseReservationFromDatabaseAsync(request.UserId, ticket.Id, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fatal] Error in TicketReservationCancelled consumer: {ex.Message}");
        }
    }
}