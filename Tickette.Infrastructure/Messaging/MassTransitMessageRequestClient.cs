using MassTransit;
using Tickette.Application.Common.Interfaces.Messaging;

namespace Tickette.Infrastructure.Messaging;

public class MassTransitMessageRequestClient : IMessageRequestClient
{
    private readonly IBus _bus;

    public MassTransitMessageRequestClient(IBus bus)
    {
        _bus = bus;
    }

    public async Task<TResponse> PublishAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : class where TResponse : class
    {
        var timeout = TimeSpan.FromSeconds(30);
        var client = _bus.CreateRequestClient<TRequest>(timeout);
        var response = await client.GetResponse<TResponse>(request, cancellationToken);

        return response.Message;
    }

    public async Task FireAndForgetAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
    {
        await _bus.Publish(message, cancellationToken);
    }
}