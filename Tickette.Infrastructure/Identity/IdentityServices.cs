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

    public async Task<string?> GetUserEmailAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.Email;
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

    public async Task<(Result Result, Guid UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new User
        {
            UserName = userName,
            Email = userName,
            Id = Guid.NewGuid()
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
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

    // Login Method
    public async Task<(Result Result, string? Token)> LoginAsync(string userName, string password)
    {
        // Validate user credentials
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return (Result.Failure(new[] { "Invalid username or password." }), null);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return (Result.Failure(new[] { "Invalid username or password." }), null);
        }

        // Generate JWT token
        var token = _tokenService.GenerateToken(user);
        return (Result.Success(), token);
    }
}

