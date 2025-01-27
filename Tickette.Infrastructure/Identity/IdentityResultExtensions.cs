using Microsoft.AspNetCore.Identity;
using Tickette.Application.Common.Models;

namespace Tickette.Infrastructure.Identity;

public static class IdentityResultExtensions
{
    public static AuthResult<T> ToApplicationResult<T>(this IdentityResult result, T data)
    {
        return result.Succeeded
            ? AuthResult<T>.Success(data)
            : AuthResult<T>.Failure(result.Errors.Select(e => e.Description));
    }
}