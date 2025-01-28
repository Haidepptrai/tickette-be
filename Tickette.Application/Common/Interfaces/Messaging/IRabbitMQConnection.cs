using RabbitMQ.Client;

namespace Tickette.Application.Common.Interfaces.Messaging;

public interface IRabbitMQConnection : IAsyncDisposable
{
    Task<IChannel> CreateChannelAsync();
}