using Microsoft.AspNetCore.Authorization;

namespace Tickette.Infrastructure.Authorization.Requirements;

public class EventRoleRequirement : IAuthorizationRequirement
{
    public string RequiredRole { get; }

    public EventRoleRequirement(string requiredRole)
    {
        RequiredRole = requiredRole;
    }
}