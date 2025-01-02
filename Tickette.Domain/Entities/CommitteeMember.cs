using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public class CommitteeMember : BaseEntity
{
    public Guid UserId { get; private set; }

    public Guid EventId { get; private set; }

    public Guid CommitteeRoleId { get; private set; }

    public DateTime JoinedAt { get; private set; }

    public User User { get; private set; }

    public CommitteeRole CommitteeRole { get; private set; }

    public Event Event { get; private set; }

    public CommitteeMember(Guid userId, Guid committeeRoleId, Guid eventId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CommitteeRoleId = committeeRoleId;
        EventId = eventId;
        JoinedAt = DateTime.UtcNow;
    }

    public void ChangeRole(Guid roleId)
    {
        CommitteeRoleId = roleId;
    }
}