using Tickette.Domain.Common;
using Tickette.Domain.Common.Exceptions;

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
        if (string.IsNullOrWhiteSpace(logo))
            throw new InvalidFieldException("Logo");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidFieldException("Name");

        if (string.IsNullOrWhiteSpace(description))
            throw new InvalidFieldException("Description");

        return new EventCommittee(logo, name, description);
    }

    public void UpdateCommittee(string logo, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(logo)) throw new InvalidFieldException("Logo");
        if (string.IsNullOrWhiteSpace(name)) throw new InvalidFieldException("Name");
        if (string.IsNullOrWhiteSpace(description)) throw new InvalidFieldException("Description");

        Logo = logo;
        Name = name;
        Description = description;
    }

}