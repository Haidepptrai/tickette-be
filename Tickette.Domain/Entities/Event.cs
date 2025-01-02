using Tickette.Domain.Common;
using Tickette.Domain.Enums;

namespace Tickette.Domain.Entities;

public class Event : BaseEntity
{
    public Guid CategoryId { get; set; }

    public string Name { get; private set; }

    public string Address { get; set; }

    public string Description { get; set; }

    public string Logo { get; set; }

    public string Banner { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string EventSlug { get; private set; }

    public ApprovalStatus Status { get; set; }

    public Category Category { get; set; }

    public EventCommittee Committee { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    private Event() { }

    public static Event CreateEvent(
        string name,
        string address,
        Guid categoryId,
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
            CategoryId = categoryId,
            Description = description,
            Logo = logo,
            Banner = banner,
            StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc),
            Status = ApprovalStatus.Pending, // Default status
            Committee = committee,
            EventSlug = GenerateSlug(name)
        };
    }

    private static string GenerateSlug(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("Name cannot be null or empty when generating a slug.");

        // Replace spaces with hyphens and convert to lowercase for the slug
        var slugBase = name.Replace(" ", "-").ToLowerInvariant();

        // Generate an 8-character GUID suffix
        var guidSuffix = Guid.NewGuid().ToString("N")[..8];

        // Combine slug base and GUID suffix
        return $"{slugBase}-{guidSuffix}";
    }
}