﻿using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Models;
using Tickette.Application.DTOs.Auth;
using Tickette.Application.Exceptions;
using Tickette.Application.Features.Auth.Command;
using Tickette.Application.Features.Users.Common;
using Tickette.Application.Wrappers;
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

    public async Task<Guid> CreateUserAsync(string fullName, string userEmail, string password)
    {
        // Check if the user already exists
        var existingUser = await _userManager.FindByEmailAsync(userEmail);

        if (existingUser != null)
        {
            return Guid.Empty;
        }

        var user = new User
        {
            UserName = userEmail,
            Email = userEmail,
            Id = Guid.NewGuid(),
            FullName = fullName,
            EmailConfirmed = false
        };

        await _userManager.CreateAsync(user, password);

        return user.Id;
    }

    public async Task<TokenRetrieval> LoginAsync(string userEmail, string password, CancellationToken cancellation)
    {
        // Validate user credentials
        var user = await _userManager.FindByEmailAsync(userEmail);

        if (user == null)
        {
            throw new AuthenticationException("Invalid credentials.");
        }

        if (!user.EmailConfirmed)
        {
            // Delete the user if the email is not confirmed
            await _userManager.DeleteAsync(user);
            throw new AuthenticationException("User not exist in system confirmed.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            throw new AuthenticationException("Invalid credentials.");
        }

        // Generate tokens
        var accessToken = await _tokenService.GenerateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token with cancellation support
        await AddOrUpdateRefreshTokenAsync(user.Id, refreshToken, cancellation);

        var resultReturn = new TokenRetrieval
        {
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            IsEmailConfirmed = user.EmailConfirmed
        };

        return resultReturn;
    }

    public async Task<bool> LogoutAsync(Guid userId, string refreshToken, CancellationToken cancellationToken)
    {
        // Validate refresh token
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new AuthenticationException("User not found.");
        }

        // Remove the refresh token
        var oldRefreshToken = user.RefreshTokens.SingleOrDefault(rt => rt.Token == refreshToken);

        if (oldRefreshToken != null)
        {
            _context.RefreshTokens.Remove(oldRefreshToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<TokenRetrieval> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        // Validate refresh token
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken), cancellationToken);

        if (user == null)
        {
            throw new AuthenticationException("Invalid refresh token.");
        }

        var oldRefreshToken = user.RefreshTokens.SingleOrDefault(rt => rt.Token == refreshToken);
        if (oldRefreshToken == null)
        {
            throw new AuthenticationException("Invalid refresh token.");
        }

        if (DateTime.UtcNow > oldRefreshToken.ExpiryTime)
        {
            // Remove the old refresh token
            _context.RefreshTokens.Remove(oldRefreshToken);
            await _context.SaveChangesAsync(cancellationToken);

            throw new AuthenticationException("Expired refresh token.");

        }

        // Generate new tokens
        var newAccessToken = await _tokenService.GenerateToken(user);

        // Update user in database
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException("Unknown Exception");
        }

        return new TokenRetrieval
        {
            UserId = user.Id,
            AccessToken = newAccessToken,
            RefreshToken = "",
        };
    }

    public async Task<bool> AssignToRoleAsync(Guid userId, IEnumerable<Guid>? roleIds)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        // If `roleIds == null` OR Empty => Remove All Roles (User is now a standard "User")
        if (roleIds == null)
        {
            var existingRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);

            if (!removeResult.Succeeded)
            {
                throw new Exception("Failed to remove roles from user.");
            }

            return true; // Successfully removed all roles (User is now a default "User")
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
            // Throw exception if any role ID is invalid
            throw new Exception("One or more role IDs are invalid. Please make sure all role IDs are correct.");
        }

        // Remove Roles That Are No Longer Assigned
        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(roles).ToList();
        if (rolesToRemove.Any())
        {
            var removeOldRolesResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeOldRolesResult.Succeeded)
            {
                // Throw exception if failed to remove roles
                throw new Exception("Failed to remove roles from user. Please make sure all role IDs are correct.");
            }
        }

        // 4️ Add New Roles
        var rolesToAdd = roles.Except(currentRoles).ToList();
        if (rolesToAdd.Any())
        {
            var addNewRolesResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addNewRolesResult.Succeeded)
            {
                // Throw exception if failed to add roles
                throw new Exception("Failed to add roles to user. Please make sure all role IDs are correct.");
            }
        }

        return true; // Successfully updated roles
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
            throw new NotFoundException("User", userId);
        }

        var roles = await _userManager.GetRolesAsync(user);

        return AuthResult<(User, List<string>)>.Success((user, roles.ToList()));
    }

    public async Task<GetUserByIdResponse> GetUserByIdWithRolesAsync(Guid userId)
    {
        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var systemRoles = await _roleManager.Roles
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Name = r.Name!
            })
            .ToListAsync();

        var userDto = user.MapToGetUserByIdResponseForAdmin(roles, systemRoles);

        return userDto;
    }


    public async Task<PagedResult<PreviewUserResponse>> GetAllUsers(
        int pageNumber, int pageSize, string? search, CancellationToken cancellationToken)
    {
        // Start with base query
        var query = _userManager.Users.AsNoTracking();

        // Apply case-insensitive search by email
        if (!string.IsNullOrWhiteSpace(search))
        {
            var loweredSearch = search.ToLower();
            query = query.Where(u => u.Email!.ToLower().Contains(loweredSearch));
        }

        // Get the count AFTER filtering
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply ordering, paging
        var users = await query
            .OrderBy(u => u.Email)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Sequential role fetching
        var userResponses = new List<PreviewUserResponse>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userResponses.Add(user.MapToPreviewUserResponse(roles));
        }

        // Wrap and return
        return new PagedResult<PreviewUserResponse>(
            userResponses,
            totalCount,
            pageNumber,
            pageSize
        );
    }

    public async Task<TokenRetrieval> SyncGoogleUserAsync(LoginWithGoogleCommand request)
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
                throw new Exception("Failed to create user.");
            }
        }

        // Generate tokens for the user
        var accessToken = await _tokenService.GenerateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save the refresh token to the database
        await AddOrUpdateRefreshTokenAsync(user.Id, refreshToken, CancellationToken.None);

        var result = new TokenRetrieval
        {
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            IsEmailConfirmed = true
        };

        return result;
    }

    public async Task<IEnumerable<RoleResponse>> GetRoleAllRoles()
    {
        var roles = await _roleManager.Roles
            .Select(r => new RoleResponse()
            {
                Id = r.Id,
                Name = r.Name!
            })
            .ToListAsync();

        return roles;
    }

    private async Task AddOrUpdateRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken)
    {
        // Remove expired tokens directly from the database
        _context.RefreshTokens.RemoveRange(
            _context.RefreshTokens.Where(rt => rt.UserId == userId && rt.ExpiryTime < DateTime.UtcNow)
        );

        var refreshTokenExpiryTime = DateTime.UtcNow.AddMonths(1);

        // Add the new refresh token
        var refreshTokenEntity = new RefreshToken(refreshToken, refreshTokenExpiryTime) { UserId = userId };
        _context.RefreshTokens.Add(refreshTokenEntity);

        // Save changes with cancellation support
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthResult<bool>> ChangeUserImageAsync(Guid userId, string image)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return AuthResult<bool>.Failure(["User not found."]);
        }
        user.ProfilePicture = image;
        var result = await _userManager.UpdateAsync(user);
        return result.ToApplicationResult(true);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        return token;
    }

    public async Task<bool> ConfirmEmailAsync(string token, string userEmail)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);

        if (user == null)
        {
            throw new UserNotFoundException();
        }

        if (user.EmailConfirmed)
            throw new AuthenticationException("User is already confirmed.");

        var result = await _userManager.ConfirmEmailAsync(user, token);

        foreach (var r in result.Errors)
        {
            Console.WriteLine(r.Description);
        }


        return result.Succeeded;
    }
}


