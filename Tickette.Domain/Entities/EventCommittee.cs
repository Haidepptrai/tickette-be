using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public class EventCommittee : BaseEntity
{
    public Guid EventId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public Event Event { get; set; }
}