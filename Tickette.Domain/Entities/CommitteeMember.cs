using Tickette.Domain.ValueObjects;

namespace Tickette.Domain.Entities;

public class CommitteeMember
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid EventId { get; private set; }

    public DateTime JoinedAt { get; private set; }

    public User User { get; private set; }

    public CommitteeRole Role { get; private set; }

    public Event Event { get; private set; }


    public CommitteeMember(Guid userId, CommitteeRole role, Guid eventId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Role = role;
        EventId = eventId;
        JoinedAt = DateTime.Now;
    }

    public void UpdateRole(CommitteeRole role)
    {
        Role = role;
    }
}