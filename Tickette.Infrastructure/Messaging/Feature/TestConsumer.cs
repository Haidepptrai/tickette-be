using MassTransit;
using Tickette.Application.Features.Orders.Command;
using Tickette.Application.Wrappers;

namespace Tickette.Infrastructure.Messaging.Feature;

public class TestConsumer : IConsumer<TestCommand>
{
    public async Task Consume(ConsumeContext<TestCommand> context)
    {
        var request = context.Message;
        try
        {
            Console.WriteLine("Hello world!");

            var result = new RedisReservationResult();
            await Task.Delay(1000);

            await context.RespondAsync<RedisReservationResult>(result);
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.Message);
            await context.RespondAsync(new RedisReservationResult());
        }
    }
}