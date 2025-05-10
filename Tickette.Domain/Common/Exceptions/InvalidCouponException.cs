namespace Tickette.Domain.Common.Exceptions;

public class InvalidCouponException : Exception
{
    public InvalidCouponException(string message) : base(message)
    {
    }
}