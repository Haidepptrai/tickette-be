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

    public static CommitteeRole Chair => new CommitteeRole("Chair");
    public static CommitteeRole Member => new CommitteeRole("Member");

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
}
