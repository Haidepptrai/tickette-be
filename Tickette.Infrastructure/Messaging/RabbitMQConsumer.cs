using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Tickette.Application.Common.Interfaces.Messaging;

namespace Tickette.Infrastructure.Messaging;

public class RabbitMQConsumer : IMessageConsumer
{
    private readonly IRabbitMQConnection _rabbitMQConnection;
    private readonly ILogger<RabbitMQConsumer> _logger;

    public RabbitMQConsumer(IRabbitMQConnection rabbitMQConnection, ILogger<RabbitMQConsumer> logger)
    {
        _rabbitMQConnection = rabbitMQConnection;
        _logger = logger;
    }

    public async Task ConsumeAsync(string queueName, Func<string, Task> onMessageReceived, CancellationToken cancellationToken)
    {
        try
        {
            // Create a channel for consuming messages
            var channel = await _rabbitMQConnection.CreateChannelAsync();

            // Declare the queue
            await channel.QueueDeclareAsync(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            // Create an async consumer
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    // Extract the message body
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    // Process the message
                    await onMessageReceived(message);

                    // Acknowledge the message after successful processing
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log the error and reject the message (optionally requeue it)
                    _logger.LogError(ex, "Error processing message.");
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken);
                }
            };

            // Start consuming messages
            await channel.BasicConsumeAsync(queue: queueName,
                autoAck: false, // Manual acknowledgment
                consumer: consumer,
                cancellationToken: cancellationToken);

            // Keep the consumer running until cancellation is requested
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken); // Add a delay to avoid tight looping
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while consuming messages.");
            throw;
        }
    }
}
