using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Models;
using Tickette.Application.DTOs.Auth;
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

    public async Task<AuthResult<object?>> AssignToRoleAsync(Guid userId, Guid roleId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return AuthResult<object?>.Failure(["User not found."]);
        }

        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return AuthResult<object?>.Failure(["Role not found."]);
        }

        var result = await _userManager.AddToRoleAsync(user, role.Name!);
        return result.ToApplicationResult<object?>(result);
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

    public async Task<AuthResult<User>> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => u.Id == userId);

        return user == null ?
            AuthResult<User>.Failure(["User not found."]) : AuthResult<User>.Success(user);
    }

    public async Task<AuthResult<User>> SyncGoogleUserAsync(GoogleUserRequest request)
    {
        // Check if the user exists in the database
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user != null) return AuthResult<User>.Success(user);

        // Create a new user if one doesn't exist
        user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            ProfilePicture = request.Image
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            return AuthResult<User>.Failure(createResult.Errors.Select(e => e.Description).ToArray());
        }

        return AuthResult<User>.Success(user);
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


