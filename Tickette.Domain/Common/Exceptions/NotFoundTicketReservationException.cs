namespace Tickette.Application.Exceptions;

public class NotFoundTicketReservationException : Exception
{
    public NotFoundTicketReservationException() : base("Cannot find your reservation, please order again")
    {
    }

}