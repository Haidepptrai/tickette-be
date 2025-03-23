using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public sealed class EventCommittee : BaseEntity
{
    public Guid EventId { get; private set; }

    public string Logo { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public Event Event { get; set; }

    protected EventCommittee() { }

    private EventCommittee(string logo, string name, string description)
    {
        Logo = logo;
        Name = name;
        Description = description;
    }

    public static EventCommittee CreateEventCommittee(string logo, string name, string description)
    {
        return new EventCommittee(logo, name, description);
    }
}