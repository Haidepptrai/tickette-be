using Tickette.Application.Common.Models;

namespace Tickette.Application.Common.Interfaces;

public interface IIdentityServices
{
    Task<string?> GetUserEmailAsync(Guid userId);

    Task<bool> IsInRoleAsync(Guid userId, string role);

    Task<bool> AuthorizeAsync(Guid userId, string policyName);

    Task<(Result Result, Guid UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(Guid userId);

    Task<(Result Result, string? Token)> LoginAsync(string userName, string password);
}
