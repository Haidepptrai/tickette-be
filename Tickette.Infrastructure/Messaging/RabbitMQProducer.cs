using RabbitMQ.Client;
using System.Text;
using Tickette.Application.Common.Interfaces.Messaging;

namespace Tickette.Infrastructure.Messaging;

public class RabbitMQProducer : IMessageProducer
{
    private readonly IRabbitMQConnection _rabbitMQConnection;

    public RabbitMQProducer(IRabbitMQConnection rabbitMQConnection)
    {
        _rabbitMQConnection = rabbitMQConnection;
    }

    public async Task<bool> PublishAsync(string queueName, string message)
    {
        try
        {
            await using var channel = await _rabbitMQConnection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                body: body);

            return true;

        }
        catch
        {
            return false;
            // Retry logic can be added here
        }
    }

}