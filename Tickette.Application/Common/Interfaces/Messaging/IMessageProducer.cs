namespace Tickette.Application.Common.Interfaces.Messaging;

public interface IMessageProducer
{
    Task<bool> PublishAsync(string queueName, string message);
}