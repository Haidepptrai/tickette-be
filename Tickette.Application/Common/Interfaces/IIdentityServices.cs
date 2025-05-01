using Tickette.Application.Common.Models;
using Tickette.Application.DTOs.Auth;
using Tickette.Application.Features.Auth.Command;
using Tickette.Application.Features.Users.Common;
using Tickette.Application.Wrappers;
using Tickette.Domain.Entities;

namespace Tickette.Application.Common.Interfaces;

public interface IIdentityServices
{
    Task<TokenRetrieval> LoginAsync(string userEmail, string password, CancellationToken cancellation);

    Task<Guid> CreateUserAsync(string fullName, string userEmail, string password);

    Task<AuthResult<TokenRetrieval>> RefreshTokenAsync(string refreshToken);

    Task<bool> AssignToRoleAsync(Guid userId, IEnumerable<Guid>? roleId);

    Task<AuthResult<object?>> DeleteUserAsync(Guid userId);

    Task<AuthResult<(User user, List<string> roles)>> GetUserByIdAsync(Guid userId);

    Task<GetUserByIdResponse> GetUserByIdWithRolesAsync(Guid userId);

    Task<PagedResult<PreviewUserResponse>> GetAllUsers(int pageNumber, int pageSize, string? search, CancellationToken cancellationToken);

    Task<TokenRetrieval> SyncGoogleUserAsync(LoginWithGoogleCommand request);

    Task<IEnumerable<RoleResponse>> GetRoleAllRoles();

    Task<AuthResult<bool>> ChangeUserImageAsync(Guid userId, string image);

    Task<string> GenerateEmailConfirmationTokenAsync(Guid userId);

    Task<bool> ConfirmEmailAsync(string token, string userEmail);
}
