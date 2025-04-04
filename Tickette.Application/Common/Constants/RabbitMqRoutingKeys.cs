namespace Tickette.Application.Common.Constants;

public static class RabbitMqRoutingKeys
{
    public const string TicketReservationCreated = "ticket-reservation-created";

    public const string TicketReservationCancelled = "ticket-reservation-cancelled";

    public const string TicketReservationConfirmed = "ticket-reservation-confirmed";
}