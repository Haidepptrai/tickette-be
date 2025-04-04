namespace Tickette.Domain.Common.Exceptions;

public class SeatOrderedException : Exception
{
    public SeatOrderedException(string message) : base(message)
    {
    }
}