using Tickette.Application.Common.Models;
using Tickette.Application.DTOs.Auth;
using Tickette.Domain.Entities;

namespace Tickette.Application.Common.Interfaces;

public interface IIdentityServices
{
    Task<AuthResult<TokenRetrieval>> LoginAsync(string userEmail, string password, CancellationToken cancellation);

    Task<AuthResult<Guid>> CreateUserAsync(string userEmail, string password);

    Task<AuthResult<TokenRetrieval>> RefreshTokenAsync(string refreshToken);

    Task<AuthResult<object?>> AssignToRoleAsync(Guid userId, Guid roleId);

    Task<AuthResult<object?>> DeleteUserAsync(Guid userId);

    Task<AuthResult<User>> GetUserByIdAsync(Guid userId);

    Task<AuthResult<User>> SyncGoogleUserAsync(GoogleUserRequest request);
}
