namespace Tickette.Application.Common.Interfaces.Messaging;

public interface IMessageProducer
{
    void Publish(string queueName, string message);
}