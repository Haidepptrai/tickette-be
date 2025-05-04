using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public sealed class CommitteeRole : BaseEntity
{
    public string Name { get; private set; }

    public ICollection<CommitteeMember> CommitteeMembers { get; set; }

    public CommitteeRole(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    public CommitteeRole(Guid id, string name) : this(name)
    {
        Id = id;
    }

    public static CommitteeRole Create(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be null or empty.", nameof(name));


        return new CommitteeRole(id, name);
    }
}

