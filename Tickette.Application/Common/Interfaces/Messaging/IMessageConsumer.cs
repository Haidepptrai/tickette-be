namespace Tickette.Application.Common.Interfaces.Messaging;

public interface IMessageConsumer
{
    void Consume(string queueName, Action<string> onMessageReceived);
}