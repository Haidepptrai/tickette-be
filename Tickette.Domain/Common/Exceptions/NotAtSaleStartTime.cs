namespace Tickette.Domain.Common.Exceptions;

public class NotAtSaleStartTime : Exception
{
    public NotAtSaleStartTime(string message) : base(message)
    {
    }
    public NotAtSaleStartTime(string message, Exception innerException) : base(message, innerException)
    {
    }
    public NotAtSaleStartTime() : base("Ticket is not at sale start time.")
    {
    }
}