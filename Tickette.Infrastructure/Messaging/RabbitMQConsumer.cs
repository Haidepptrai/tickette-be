using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Tickette.Application.Common.Interfaces.Messaging;

namespace Tickette.Infrastructure.Messaging;

public class RabbitMQConsumer : IMessageConsumer
{
    private readonly IRabbitMQConnection _rabbitMQConnection;

    public RabbitMQConsumer(IRabbitMQConnection rabbitMQConnection)
    {
        _rabbitMQConnection = rabbitMQConnection;
    }

    public async void Consume(string queueName, Action<string> onMessageReceived)
    {
        try
        {
            await using var channel = await _rabbitMQConnection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Invoke the callback action
                onMessageReceived(message);
                return Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue: queueName,
                autoAck: true,
                consumer: consumer);
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while consuming the message: {e.Message}");
        }
    }
}
