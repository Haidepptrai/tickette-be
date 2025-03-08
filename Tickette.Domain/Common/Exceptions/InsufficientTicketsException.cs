namespace Tickette.Application.Exceptions;

public class InsufficientTicketsException : Exception
{
    public InsufficientTicketsException() : base("Not enough quantity of the ticket for you to reserve")
    {
    }
}