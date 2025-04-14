namespace Tickette.Domain.Common.Exceptions;

public class ValidateFieldException : Exception
{
    public ValidateFieldException(string fieldName, string message)
        : base($"Validation failed for field '{fieldName}': {message}")
    {
    }
}