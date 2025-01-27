using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public sealed class EventCommittee : BaseEntity
{
    public Guid EventId { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public Event Event { get; set; }

    protected EventCommittee() { }

    private EventCommittee(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public static EventCommittee CreateEventCommittee(string name, string description)
    {
        return new EventCommittee(name, description);
    }
}