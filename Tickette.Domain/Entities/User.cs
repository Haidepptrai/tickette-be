using Microsoft.AspNetCore.Identity;
using Tickette.Domain.Enums;

namespace Tickette.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string? FullName { get; set; }

    public string? ProfilePicture { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Gender Gender { get; set; } = Gender.Other;

    //RefreshToken
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
}
