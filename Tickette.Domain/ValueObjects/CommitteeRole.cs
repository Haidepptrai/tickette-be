namespace Tickette.Domain.ValueObjects;

public class CommitteeRole : IEquatable<CommitteeRole>
{
    public string Name { get; }

    private CommitteeRole(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Role name cannot be empty or null.", nameof(name));
        }

        Name = name;
    }

    // Predefined roles
    public static CommitteeRole Chair => new CommitteeRole("Event Chair");
    public static CommitteeRole Admin => new CommitteeRole("Administrator");
    public static CommitteeRole Manager => new CommitteeRole("Manager");
    public static CommitteeRole CheckInStaff => new CommitteeRole("Staff (Check In)");
    public static CommitteeRole CheckOutStaff => new CommitteeRole("Staff (Check Out)");
    public static CommitteeRole RedeemStaff => new CommitteeRole("Staff (Redeem)");

    // List of all roles for validation or iteration
    public static IEnumerable<CommitteeRole> AllRoles => new[]
    {
        Chair,
        Admin,
        Manager,
        CheckInStaff,
        CheckOutStaff,
        RedeemStaff
    };

    // Equality logic
    public override bool Equals(object? obj)
    {
        return Equals(obj as CommitteeRole);
    }

    public bool Equals(CommitteeRole? other)
    {
        if (other == null) return false;
        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }

    // Factory method for flexible creation
    public static CommitteeRole FromName(string name)
    {
        var role = AllRoles.FirstOrDefault(r => r.Name == name);
        if (role == null)
        {
            throw new ArgumentException($"Invalid role name: {name}");
        }

        return role;
    }
}