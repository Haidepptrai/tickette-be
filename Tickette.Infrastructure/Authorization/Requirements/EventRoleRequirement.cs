using Microsoft.AspNetCore.Authorization;

namespace Tickette.Infrastructure.Authorization.Requirements;

public class EventRoleRequirement : IAuthorizationRequirement
{
    public HashSet<string> RequiredRoles { get; }

    public EventRoleRequirement(string primaryRole, params string[] elevatedRoles)
    {
        RequiredRoles = [primaryRole];
        RequiredRoles.UnionWith(elevatedRoles); // Include Admin and EventOwner
    }
}
