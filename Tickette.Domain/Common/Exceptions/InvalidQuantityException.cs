namespace Tickette.Application.Exceptions;

public class InvalidQuantityException : Exception
{
    public InvalidQuantityException()
        : base("Invalid quantity for ticket")
    {
    }

}