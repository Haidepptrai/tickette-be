using Microsoft.AspNetCore.Identity;
using Tickette.Domain.Enums;

namespace Tickette.Domain.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string ProfilePicture { get; set; }

    public DateTime DoB { get; set; }

    public Gender Gender { get; set; }
}
