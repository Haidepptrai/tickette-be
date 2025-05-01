using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Command.CreateOrder;

namespace Tickette.Infrastructure.Messaging.Feature;

public class ConfirmTicketReservationConsumer : IConsumer<CreateOrderCommand>
{
    private readonly IServiceProvider _serviceProvider;

    public ConfirmTicketReservationConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Consume(ConsumeContext<CreateOrderCommand> context)
    {
        try
        {
            var request = context.Message;

            using var scope = _serviceProvider.CreateScope();
            var redisHandler = scope.ServiceProvider.GetRequiredService<IReservationService>();

            foreach (var ticket in request.Tickets)
            {
                // 1. Remove from Redis
                await redisHandler.FinalizeSeatReservationAsync(request.UserId, ticket);

                // 2. Mark reservation as confirmed in DB
                //await dbHandler.ConfirmReservationInDatabaseAsync(request.UserId, ticket.Id);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fatal] Error in TicketReservationCancelled consumer: {ex.Message}");
            // Log and exit gracefully — don't rethrow
        }
    }
}