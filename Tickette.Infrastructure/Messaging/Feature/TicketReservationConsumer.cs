using System.Text.Json;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Features.Orders.Command.ReverseTicket;
using Tickette.Domain.Common;

namespace Tickette.Infrastructure.Messaging.Feature;

public class TicketReservationConsumer
{
    private readonly IMessageConsumer _messageConsumer;

    public TicketReservationConsumer(IMessageConsumer messageConsumer)
    {
        _messageConsumer = messageConsumer;
    }

    public void StartListening()
    {
        _messageConsumer.Consume(Constant.TICKET_RESERVATION_QUEUE, HandleTicketReservation);
    }

    private void HandleTicketReservation(string message)
    {
        // Deserialize the message and handle the event
        var ticketReservation = JsonSerializer.Deserialize<ReverseTicketCommand>(message);
        if (ticketReservation != null)
            Console.WriteLine($"Handling ticket reservation for UserId: {ticketReservation.UserId}");
        // Business logic to handle ticket reservation...
    }
}