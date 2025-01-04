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

    // Predefined roles with permissions
    public static CommitteeRole EventOwner => new CommitteeRole(Constant.COMMITTEE_MEMBER_ROLES.EventOwner);

    public static CommitteeRole Admin => new CommitteeRole(Constant.COMMITTEE_MEMBER_ROLES.Admin);

    public static CommitteeRole Manager => new CommitteeRole(Constant.COMMITTEE_MEMBER_ROLES.Manager);

    public static CommitteeRole CheckInStaff => new CommitteeRole(Constant.COMMITTEE_MEMBER_ROLES.CheckInStaff);

    public static CommitteeRole CheckOutStaff => new CommitteeRole(Constant.COMMITTEE_MEMBER_ROLES.CheckOutStaff);

    public static CommitteeRole RedeemStaff => new CommitteeRole(Constant.COMMITTEE_MEMBER_ROLES.RedeemStaff);

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

    public void AddPermission(CommitteeRolePermission permission)
    {
        if (!Permissions.Contains(permission))
            Permissions.Add(permission);
    }
}

