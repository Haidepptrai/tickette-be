namespace Tickette.Domain.Common.Exceptions;

public class InvalidReservationException : Exception
{
    public InvalidReservationException() : base("Invalid reservation")
    {
    }
}