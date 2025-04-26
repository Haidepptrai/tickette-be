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

    public async Task ConsumeAsync(string queueName, Func<string, Task<string>> onMessageReceived, CancellationToken cancellationToken)
    {
        try
        {
            var channel = await _rabbitMQConnection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                const int maxAttempts = 3;
                string? response = null;

                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        // Call your handler and get the result
                        response = await onMessageReceived(message);

                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken);
                        break; // Success
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Attempt {Attempt} failed while processing message.", attempt);

                        if (attempt == maxAttempts)
                        {
                            _logger.LogWarning("Max retry attempts reached. Requeuing message.");
                            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken);
                            return;
                        }

                        var backoff = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                        await Task.Delay(backoff, cancellationToken);
                    }
                }

                // Handle RPC response
                if (!string.IsNullOrWhiteSpace(ea.BasicProperties?.ReplyTo) && !string.IsNullOrWhiteSpace(ea.BasicProperties.CorrelationId) && response is not null)
                {
                    var props = new BasicProperties
                    {
                        CorrelationId = ea.BasicProperties.CorrelationId
                    };

                    ReadOnlyMemory<byte> replyBody = Encoding.UTF8.GetBytes(response);
                    await channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: ea.BasicProperties.ReplyTo,
                        mandatory: false,
                        body: replyBody,
                        basicProperties: props, cancellationToken: cancellationToken);
                }
            };

            await channel.BasicConsumeAsync(queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while consuming messages.");
            throw;
        }
    }
}
