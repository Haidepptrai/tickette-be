namespace Tickette.Application.Common.Interfaces.Messaging;

public interface IMessageRequestClient
{
    Task<TResponse> PublishAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : class
        where TResponse : class;

    Task FireAndForgetAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : class;
}
