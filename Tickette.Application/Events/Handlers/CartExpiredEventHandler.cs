using System.Text.Json;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Events.Models;
using Tickette.Domain.Common;

namespace Tickette.Application.Events.Handlers;

public class CartExpiredEventHandler
{
    private readonly IMessageConsumer _messageConsumer;

    public CartExpiredEventHandler(IMessageConsumer messageConsumer)
    {
        _messageConsumer = messageConsumer;
    }

    public void StartListening()
    {
        _messageConsumer.Consume(Constant.CART_EXPIRED_QUEUE, HandleCartExpired);
    }

    private void HandleCartExpired(string message)
    {
        // Deserialize the message and handle the event
        var cartExpired = JsonSerializer.Deserialize<CartExpiredEvent>(message);
        Console.WriteLine($"Handling cart expiration for CartId: {cartExpired.CartId}");

        // Business logic to handle cart expiration...
    }

}