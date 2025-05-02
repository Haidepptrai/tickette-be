namespace Tickette.Domain.Common.Exceptions;

public class InvalidSeatLogicSelection : Exception
{
    public InvalidSeatLogicSelection(string message) : base(message)
    {
    }
}