using Microsoft.AspNetCore.Identity;
using Tickette.Domain.Enums;

namespace Tickette.Domain.Entities;

public sealed class User : IdentityUser<Guid>
{
    public string? FullName { get; set; }

    public string? ProfilePicture { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Gender Gender { get; set; } = Gender.Other;

    public List<Event> Events { get; private set; } = new();

    public List<RefreshToken> RefreshTokens { get; private set; } = new();

    public void AddRefreshToken(string token, DateTime expiryTime)
    {
        // Optionally: Remove expired tokens to keep the list clean
        RefreshTokens.RemoveAll(rt => rt.ExpiryTime <= DateTime.UtcNow);

        RefreshTokens.Add(new RefreshToken(token, expiryTime));
    }
}
