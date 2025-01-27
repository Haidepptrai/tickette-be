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

    private CommitteeMember() { }

    private CommitteeMember(User user, CommitteeRole role, Event originalEvent)
    {
        User = user;
        CommitteeRole = role;
        Event = originalEvent;
    }

    private CommitteeMember(Guid userId, Guid roleId, Guid eventId)
    {
        UserId = userId;
        CommitteeRoleId = roleId;
        EventId = eventId;
    }

    public static CommitteeMember Create(User user, CommitteeRole role, Event originalEvent)
    {
        return new CommitteeMember(user, role, originalEvent);
    }

    public static CommitteeMember Create(Guid userId, Guid roleId, Guid eventId)
    {
        return new CommitteeMember(userId, roleId, eventId);
    }

    public void ChangeRole(Guid roleId)
    {
        CommitteeRoleId = roleId;
    }
}