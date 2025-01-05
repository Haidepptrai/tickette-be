using Tickette.Domain.Common;
using Tickette.Domain.ValueObjects;

namespace Tickette.Domain.Entities;

public sealed class CommitteeRole : BaseEntity
{
    public string Name { get; private set; }

    public List<CommitteeRolePermission> Permissions { get; private set; } = [];

    public ICollection<CommitteeMember> CommitteeMembers { get; private set; }

    public CommitteeRole(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    // Method to add permissions
    public CommitteeRole WithPermissions(params string[] permissions)
    {
        foreach (var permission in permissions)
        {
            var permissionObj = new CommitteeRolePermission(permission);
            if (!Permissions.Contains(permissionObj))
            {
                Permissions.Add(permissionObj);
            }
        }
        return this;
    }

    // Check if role has a specific permission
    public bool HasPermission(string permissionName)
    {
        return Permissions.Any(p => p.Name == permissionName);
    }
}

