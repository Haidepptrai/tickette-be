using Tickette.Application.Common.Models;
using Tickette.Application.DTOs.Auth;
using Tickette.Application.Features.Users.Common;
using Tickette.Domain.Entities;

namespace Tickette.Application.Common.Interfaces;

public interface IIdentityServices
{
    Task<AuthResult<TokenRetrieval>> LoginAsync(string userEmail, string password, CancellationToken cancellation);

    Task<AuthResult<Guid>> CreateUserAsync(string userEmail, string password);

    Task<AuthResult<TokenRetrieval>> RefreshTokenAsync(string refreshToken);

    Task<AuthResult<object?>> AssignToRoleAsync(Guid userId, IEnumerable<Guid>? roleId);

    Task<AuthResult<object?>> DeleteUserAsync(Guid userId);

    Task<AuthResult<(User user, List<string> roles)>> GetUserByIdAsync(Guid userId);

    Task<AuthResult<IEnumerable<PreviewUserResponse>>> GetAllUsers(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<AuthResult<TokenRetrieval>> SyncGoogleUserAsync(GoogleUserRequest request);

    Task<IEnumerable<RoleResponse>> GetRoleIds();

    Task<User?> FindUserByEmailAsync(string email);
}
