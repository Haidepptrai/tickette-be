using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Tickette.Application.Common.Interfaces.Messaging;

namespace Tickette.Infrastructure.Messaging;

public class RabbitMQProducer : IMessageProducer
{
    private readonly IRabbitMQConnection _rabbitMQConnection;
    private readonly ILogger<RabbitMQProducer> _logger;

    public RabbitMQProducer(IRabbitMQConnection rabbitMQConnection, ILogger<RabbitMQProducer> logger)
    {
        _rabbitMQConnection = rabbitMQConnection;
        _logger = logger;
    }

    public async Task<string?> PublishAsync(string queueName, string message, TimeSpan? replyTimeout = null)
    {
        var attempt = 0;

        while (attempt < 3)
        {
            try
            {
                await using var channel = await _rabbitMQConnection.CreateChannelAsync();

                await channel.QueueDeclareAsync(queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                ReadOnlyMemory<byte> body = Encoding.UTF8.GetBytes(message);

                if (replyTimeout.HasValue)
                {
                    // === Reply-Expected Mode ===
                    var replyQueue = await channel.QueueDeclareAsync("", durable: false, exclusive: true, autoDelete: true);
                    var replyQueueName = replyQueue.QueueName;

                    var tcs = new TaskCompletionSource<string>();
                    var correlationId = Guid.NewGuid().ToString();

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.ReceivedAsync += async (_, ea) =>
                    {
                        if (ea.BasicProperties?.CorrelationId == correlationId)
                        {
                            var response = Encoding.UTF8.GetString(ea.Body.ToArray());
                            tcs.TrySetResult(response);
                        }

                        await Task.CompletedTask;
                    };

                    await channel.BasicConsumeAsync(queue: replyQueueName, autoAck: true, consumer: consumer);

                    var props = new BasicProperties
                    {
                        CorrelationId = correlationId,
                        ReplyTo = replyQueueName
                    };

                    await channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: queueName,
                        mandatory: false,
                        body: body,
                        basicProperties: props
                    );

                    using var cts = new CancellationTokenSource(replyTimeout.Value);
                    await using (cts.Token.Register(() => tcs.TrySetCanceled()))
                    {
                        return await tcs.Task;
                    }
                }
                else
                {
                    // === Fire-and-Forget Mode ===
                    await channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: queueName,
                        mandatory: false,
                        body: body
                    );

                    return null; // No response expected
                }
            }
            catch (Exception ex)
            {
                attempt++;

                if (attempt >= 3)
                {
                    _logger.LogError(ex, $"Failed to publish to {queueName} after {attempt} attempts.");
                    return null;
                }

                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
        }

        return null; // Should never hit this
    }

}