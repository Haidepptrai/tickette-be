using RabbitMQ.Client;
using Tickette.Application.Common.Interfaces.Messaging;

namespace Tickette.Infrastructure.Messaging;

public class RabbitMQConnection : IRabbitMQConnection, IAsyncDisposable
{
    private IConnection? _connection; // Nullable since the connection is created asynchronously
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMQConnection(RabbitMQSettings settings)
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password
        };
    }

    /// <summary>
    /// Asynchronously creates a RabbitMQ channel.
    /// </summary>
    public async Task<IChannel> CreateChannelAsync()
    {
        if (_connection is not { IsOpen: true })
        {
            _connection = await _connectionFactory.CreateConnectionAsync(); // Create connection asynchronously
        }

        return await _connection.CreateChannelAsync(); // Create and return the channel
    }

    /// <summary>
    /// Asynchronously disposes the RabbitMQ connection.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_connection is { IsOpen: true })
        {
            await _connection.CloseAsync(); // Close the connection
            await Task.Run(() => _connection.Dispose()); // Dispose asynchronously
        }
    }
}
