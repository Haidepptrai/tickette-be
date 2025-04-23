namespace Tickette.Application.Common.Interfaces.Messaging;

public interface IMessageProducer
{
    Task<string?> PublishAsync(string queueName, string message, TimeSpan? replyTimeout = null);
}