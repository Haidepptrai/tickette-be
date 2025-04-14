namespace Tickette.Domain.Common.Exceptions;

public class InvalidFieldException : Exception
{
    public InvalidFieldException(string fieldName)
        : base($"The field '{fieldName}' is missing or invalid.")
    {
    }
}