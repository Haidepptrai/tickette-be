using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public sealed class CommitteeMember : BaseEntity
{
    public Guid UserId { get; private set; }

    public Guid EventId { get; private set; }

    public Guid CommitteeRoleId { get; private set; }

    public DateTime JoinedAt { get; private set; }

    public User User { get; private set; }

    public CommitteeRole CommitteeRole { get; private set; }

    public Event Event { get; private set; }

    private CommitteeMember(Guid userId, Guid committeeRoleId, Guid eventId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CommitteeRoleId = committeeRoleId;
        EventId = eventId;
        JoinedAt = DateTime.UtcNow;
    }

    // Factory Method
    public static CommitteeMember Create(Guid userId, Guid committeeRoleId, Guid eventId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.");

        if (committeeRoleId == Guid.Empty)
            throw new ArgumentException("Committee Role ID cannot be empty.");

        if (eventId == Guid.Empty)
            throw new ArgumentException("Event ID cannot be empty.");

        return new CommitteeMember(userId, committeeRoleId, eventId);
    }

    public void ChangeRole(Guid roleId)
    {
        CommitteeRoleId = roleId;
    }
}