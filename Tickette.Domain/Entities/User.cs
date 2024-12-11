using Microsoft.AspNetCore.Identity;

namespace Tickette.Domain.Entities;

public class User : IdentityUser
{
    public string ProfilePicture { get; set; }
}
