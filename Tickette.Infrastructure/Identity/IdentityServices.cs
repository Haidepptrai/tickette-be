using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        var refreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Adjust expiry time as needed

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
        // Step 1: Extract user information from the expired access token
        var principal = _tokenService.GetPrincipalFromExpiredToken(token);
        if (principal == null) // If the token is invalid
        {
            return (Result.Failure(new[] { "Invalid access token." }), null, null);
        }

        // Step 2: Extract user ID from the token claims
        var userId = principal.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        if (userId == null) // If the user ID isn't in the token
        {
            return (Result.Failure(new[] { "Invalid token payload." }), null, null);
        }

        // Step 3: Find the user in the database
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            // If the user does not exist, the refresh token doesn’t match, or it has expired
            return (Result.Failure(new[] { "Invalid or expired refresh token." }), null, null);
        }

        // Step 4: Generate a new access token
        var newAccessToken = _tokenService.GenerateToken(user);

        // Step 5: Generate a new refresh token (for token rotation)
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Set new expiry time

        // Save the new refresh token in the database
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded) // If updating the user fails
        {
            return (Result.Failure(updateResult.Errors.Select(e => e.Description).ToArray()), null, null);
        }

        // Step 6: Return the new tokens
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

