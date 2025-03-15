using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Models;
using Tickette.Application.DTOs.Auth;
using Tickette.Application.Features.Users.Common;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Identity;

public class IdentityServices : IIdentityServices
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IApplicationDbContext _context;

    public IdentityServices(
        UserManager<User> userManager,
        ITokenService tokenService,
        SignInManager<User> signInManager,
        RoleManager<IdentityRole<Guid>> roleManager, IApplicationDbContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<AuthResult<Guid>> CreateUserAsync(string userEmail, string password)
    {
        var user = new User
        {
            UserName = userEmail,
            Email = userEmail,
            Id = Guid.NewGuid()
        };

        var result = await _userManager.CreateAsync(user, password);

        return result.ToApplicationResult(user.Id);
    }

    public async Task<AuthResult<TokenRetrieval>> LoginAsync(string userEmail, string password, CancellationToken cancellation)
    {
        // Validate user credentials
        var user = await _userManager
            .FindByEmailAsync(userEmail);
        if (user == null)
        {
            return AuthResult<TokenRetrieval>.Failure(["Invalid credentials."]);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return AuthResult<TokenRetrieval>.Failure(["Invalid credentials."]);
        }

        // Generate tokens
        var accessToken = await _tokenService.GenerateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);

        // Save refresh token with cancellation support
        await AddOrUpdateRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiryTime, cancellation);

        return AuthResult<TokenRetrieval>.Success(new TokenRetrieval
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    public async Task<AuthResult<TokenRetrieval>> RefreshTokenAsync(string refreshToken)
    {
        // Validate refresh token
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken));

        if (user == null)
        {
            return AuthResult<TokenRetrieval>.Failure(["Invalid refresh token."]);
        }

        var token = user.RefreshTokens.SingleOrDefault(rt => rt.Token == refreshToken);
        if (token == null || token.ExpiryTime <= DateTime.UtcNow)
        {
            return AuthResult<TokenRetrieval>.Failure(["Invalid or expired refresh token."]);
        }

        // Generate new tokens
        var newAccessToken = await _tokenService.GenerateToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newExpiryTime = DateTime.UtcNow.AddMonths(1);

        user.RefreshTokens.Remove(token);
        user.AddRefreshToken(newRefreshToken, newExpiryTime);

        // Update user in database
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return AuthResult<TokenRetrieval>.Failure(updateResult.Errors.Select(e => e.Description));
        }

        return AuthResult<TokenRetrieval>.Success(new TokenRetrieval
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }

    public async Task<AuthResult<object?>> AssignToRoleAsync(Guid userId, IEnumerable<Guid>? roleIds)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return AuthResult<object?>.Failure(["User not found."]);
        }

        // If `roleIds == null` OR Empty => Remove All Roles (User is now a standard "User")
        if (roleIds == null)
        {
            var existingRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);

            if (!removeResult.Succeeded)
            {
                return removeResult.ToApplicationResult<object?>(removeResult);
            }

            return AuthResult<object?>.Success(); // Successfully removed all roles (User is now a default "User")
        }

        // Validate All Role IDs Exist Before Assigning
        var roles = await _roleManager.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name) // This produces `List<string?>`
            .Where(name => name != null) // Filter out null values
            .Select(name => name!) // Convert to `List<string>` -> Just to avoid null issue
            .ToListAsync();

        if (roles.Count != roleIds.Count())
        {
            return AuthResult<object?>.Failure(["One or more roles not found."]);
        }

        // Remove Roles That Are No Longer Assigned
        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(roles).ToList();
        if (rolesToRemove.Any())
        {
            var removeOldRolesResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeOldRolesResult.Succeeded)
            {
                return removeOldRolesResult.ToApplicationResult<object?>(removeOldRolesResult);
            }
        }

        // 4️ Add New Roles
        var rolesToAdd = roles.Except(currentRoles).ToList();
        if (rolesToAdd.Any())
        {
            var addNewRolesResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addNewRolesResult.Succeeded)
            {
                return addNewRolesResult.ToApplicationResult<object?>(addNewRolesResult);
            }
        }

        return AuthResult<object?>.Success(null); // Successfully updated roles
    }


    public async Task<AuthResult<object?>> DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return AuthResult<object?>.Failure(["User not found."]);
        }

        var result = await _userManager.DeleteAsync(user);
        return result.ToApplicationResult<object?>(result);
    }

    public async Task<AuthResult<(User user, List<string> roles)>> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return AuthResult<(User, List<string>)>.Failure(["User not found."]);
        }

        var roles = await _userManager.GetRolesAsync(user);

        return AuthResult<(User, List<string>)>.Success((user, roles.ToList()));
    }


    public async Task<AuthResult<IEnumerable<PreviewUserResponse>>> GetAllUsers(
        int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        // Fetch paginated users first
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (!users.Any())
            return AuthResult<IEnumerable<PreviewUserResponse>>.Failure(["Users not found."]);

        // Fetch roles sequentially to avoid DbContext threading issues
        var userResponses = new List<PreviewUserResponse>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user); // Fetch roles one by one
            userResponses.Add(user.MapToPreviewUserResponse(roles));
        }

        return AuthResult<IEnumerable<PreviewUserResponse>>.Success(userResponses);
    }

    public async Task<AuthResult<TokenRetrieval>> SyncGoogleUserAsync(GoogleUserRequest request)
    {
        // Check if the user exists in the database
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Create a new user if one doesn't exist
            user = new User
            {
                FullName = request.Name,
                Email = request.Email,
                EmailConfirmed = true,
                ProfilePicture = request.Image,
                UserName = request.Email
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return AuthResult<TokenRetrieval>.Failure(createResult.Errors.Select(e => e.Description).ToArray());
            }
        }

        // Generate tokens for the user
        var accessToken = await _tokenService.GenerateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);

        // Save the refresh token to the database
        await AddOrUpdateRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiryTime, CancellationToken.None);

        return AuthResult<TokenRetrieval>.Success(new TokenRetrieval
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    public async Task<IEnumerable<RoleResponse>> GetRoleIds()
    {
        var roleIds = await _roleManager.Roles
            .Select(r => new RoleResponse()
            {
                Id = r.Id,
                Name = r.Name!
            })
            .ToListAsync();

        return roleIds;
    }

    private async Task AddOrUpdateRefreshTokenAsync(Guid userId, string token, DateTime expiryTime, CancellationToken cancellationToken)
    {
        // Remove expired tokens directly from the database
        _context.RefreshTokens.RemoveRange(
            _context.RefreshTokens.Where(rt => rt.UserId == userId && rt.ExpiryTime <= DateTime.UtcNow)
        );

        // Add the new refresh token
        var refreshToken = new RefreshToken(token, expiryTime) { UserId = userId };
        _context.RefreshTokens.Add(refreshToken);

        // Save changes with cancellation support
        await _context.SaveChangesAsync(cancellationToken);
    }
}


