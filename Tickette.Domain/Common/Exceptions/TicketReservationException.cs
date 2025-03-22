namespace Tickette.Domain.Common.Exceptions;

public class TicketReservationException : Exception
{
    public TicketReservationException(string message) : base(message)
    {
    }
}