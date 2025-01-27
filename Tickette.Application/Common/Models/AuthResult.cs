namespace Tickette.Application.Common.Models;

public class AuthResult<T>
{
    public bool Succeeded { get; init; }

    public string[] Errors { get; init; }

    public T? Data { get; init; }

    internal AuthResult(bool succeeded, IEnumerable<string> errors, T? data = default)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        Data = data;
    }

    public static AuthResult<T> Success(T? data = default)
    {
        return new AuthResult<T>(true, Array.Empty<string>(), data);
    }

    public static AuthResult<T> Failure(IEnumerable<string> errors)
    {
        return new AuthResult<T>(false, errors);
    }
}