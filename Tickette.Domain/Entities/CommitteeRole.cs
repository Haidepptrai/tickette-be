using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public sealed class CommitteeRole : BaseEntity
{
    public string Name { get; private set; }

    public ICollection<CommitteeMember> CommitteeMembers { get; private set; }

    public CommitteeRole(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
}

