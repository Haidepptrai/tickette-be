using Tickette.Domain.Enums;

namespace Tickette.Domain.Entities;

public class Event
{
    public Guid Id { get; }

    public string Name { get; private set; }

    public string Address { get; set; }

    public EventType Type { get; set; }

    public string Description { get; set; }

    public string Logo { get; set; }

    public string Banner { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public ApprovalStatus Status { get; set; }

    public EventCommittee Committee { get; set; }

    public DateTime DeletedAt { get; set; }

    private Event() { }

    public static Event CreateEvent(
        string name,
        string address,
        EventType type,
        string description,
        string logo,
        string banner,
        DateTime startDate,
        DateTime endDate,
        EventCommittee committee)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Event name cannot be empty.", nameof(name));

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be earlier than end date.", nameof(startDate));

        return new Event
        {
            Name = name,
            Address = address,
            Type = type,
            Description = description,
            Logo = logo,
            Banner = banner,
            StartDate = startDate,
            EndDate = endDate,
            Status = ApprovalStatus.Pending, // Default status
            Committee = committee,
        };
    }
}