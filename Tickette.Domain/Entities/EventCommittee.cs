namespace Tickette.Domain.Entities;

public class EventCommittee
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public Event Event { get; set; }
}