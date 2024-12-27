using Tickette.Application.Common.Models;
namespace Tickette.Application.Common.Interfaces;

public interface IIdentityServices
{
    Task<(Result Result, string? AccessToken, string? RefreshToken)> LoginAsync(string userEmail, string password);

    Task<(Result Result, Guid UserId)> CreateUserAsync(string userEmail, string password);

    Task<(Result Result, string? AccessToken, string? RefreshToken)> RefreshTokenAsync(string token, string refreshToken);

    Task<bool> IsInRoleAsync(Guid userId, string role);

    Task<bool> AuthorizeAsync(Guid userId, string policyName);

    Task<Result> DeleteUserAsync(Guid userId);

}
