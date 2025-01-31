namespace Tickette.Application.Common.Interfaces.Messaging;

public interface IMessageConsumer
{
    Task ConsumeAsync(string queueName, Func<string, Task> onMessageReceived, CancellationToken cancellationToken);
}
