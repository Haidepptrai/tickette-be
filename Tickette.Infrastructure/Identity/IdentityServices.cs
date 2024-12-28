using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Models;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Identity;

public class IdentityServices : IIdentityServices
{
    private readonly UserManager<User> _userManager;
    private readonly IUserClaimsPrincipalFactory<User> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly ITokenService _tokenService;
    private readonly SignInManager<User> _signInManager;

    public IdentityServices(
        UserManager<User> userManager,
        IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        ITokenService tokenService,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _authorizationService = authorizationService;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _tokenService = tokenService;
        _signInManager = signInManager;
    }

    public async Task<bool> IsInRoleAsync(Guid userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        return await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(Guid userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<(Result Result, Guid UserId)> CreateUserAsync(string userEmail, string password)
    {
        var user = new User
        {
            UserName = userEmail,
            Email = userEmail,
            Id = Guid.NewGuid()
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    // Login Method
    public async Task<(Result Result, string? AccessToken, string? RefreshToken)> LoginAsync(string userEmail, string password)
    {
        // Validate user credentials
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            return (Result.Failure(["Invalid Credential."]), null, null);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return (Result.Failure(["Invalid Credential."]), null, null);
        }

        // Generate Access Token
        var accessToken = _tokenService.GenerateToken(user);

        // Generate a new Refresh Token and set expiry time
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);

        // Update user's Refresh Token and Expiry
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiryTime;

        // Save changes to the database
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return (Result.Failure(updateResult.Errors.Select(e => e.Description).ToArray()), null, null);
        }

        // Return token and refresh token
        return (Result.Success(), accessToken, refreshToken);
    }

    public async Task<(Result Result, string? AccessToken, string? RefreshToken)> RefreshTokenAsync(string token, string refreshToken)
    {
        // Validate the token (optional if you're just replacing the expired Access Token)
        var principal = _tokenService.GetPrincipalFromExpiredToken(token);
        if (principal == null)
        {
            return (Result.Failure(["Invalid token."]), null, null);
        }

        // Extract the user ID from the claims
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return (Result.Failure(["Invalid token."]), null, null);
        }

        // Find the user by ID
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return (Result.Failure(["Invalid or expired refresh token."]), null, null);
        }

        // Generate a new Access Token
        var newAccessToken = _tokenService.GenerateToken(user);

        // Generate a new Refresh Token
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);

        // Update the user in the database
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return (Result.Failure(updateResult.Errors.Select(e => e.Description).ToArray()), null, null);
        }

        // Return the new tokens
        return (Result.Success(), newAccessToken, newRefreshToken);
    }

    public async Task<Result> DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            return Result.Failure(new[] { "User not found." });
        }

        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }
}

