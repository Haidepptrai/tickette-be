using Tickette.Domain.Common;
using Tickette.Domain.Enums;

namespace Tickette.Domain.Entities;

public sealed class Event : BaseEntity
{
    public Guid CategoryId { get; set; }

    public Guid CreatedById { get; set; }

    public string Name { get; private set; }

    public string LocationName { get; set; }

    public string City { get; set; }

    public string District { get; set; }

    public string Ward { get; set; }

    public string StreetAddress { get; set; }

    public string Description { get; set; }

    public string Banner { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string EventSlug { get; private set; }

    public ApprovalStatus Status { get; set; }

    public string EventOwnerStripeId { get; init; }

    public Category Category { get; set; }

    public EventCommittee Committee { get; set; }

    public User User { get; set; }

    public ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();

    public ICollection<CommitteeMember> CommitteeMembers { get; set; } = new List<CommitteeMember>();

    public ICollection<EventDate> EventDates { get; set; } = new List<EventDate>();

    protected Event() { }

    private Event(
        string name,
        string locationName,
        string city,
        string district,
        string ward,
        string streetAddress,
        string description,
        string banner,
        string eventOwnerStripeId,
        Guid categoryId,
        DateTime startDate,
        DateTime endDate,
        User userCreated,
        EventCommittee committee
        )
    {
        Name = name;
        LocationName = locationName;
        City = city;
        District = district;
        Ward = ward;
        StreetAddress = streetAddress;
        CategoryId = categoryId;
        Description = description;
        Banner = banner;
        EventOwnerStripeId = eventOwnerStripeId;
        StartDate = startDate;
        EndDate = endDate;
        User = userCreated;
        Committee = committee;
        CommitteeMembers = new List<CommitteeMember>();
        EventSlug = GenerateSlug(name);
        Status = ApprovalStatus.Pending;
    }

    public static Event CreateEvent(
        string name,
        string locationName,
        string city,
        string district,
        string ward,
        string streetAddress,
        string description,
        string banner,
        string eventOwnerStripeId,
        Guid categoryId,
        DateTime startDate,
        DateTime endDate,
        User userCreated,
        EventCommittee committee
        )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Event name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(locationName))
            throw new ArgumentException("Location name cannot be empty.", nameof(locationName));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));

        if (string.IsNullOrWhiteSpace(district))
            throw new ArgumentException("District cannot be empty.", nameof(district));

        if (string.IsNullOrWhiteSpace(ward))
            throw new ArgumentException("Ward cannot be empty.", nameof(ward));

        if (string.IsNullOrWhiteSpace(streetAddress))
            throw new ArgumentException("Street address cannot be empty.", nameof(streetAddress));

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be earlier than end date.", nameof(startDate));

        var committeeCreation = EventCommittee.CreateEventCommittee(committee.Logo, committee.Name, committee.Description);

        return new Event(
            name,
            locationName,
            city,
            district,
            ward,
            streetAddress,
            description,
            banner,
            eventOwnerStripeId,
            categoryId,
            startDate,
            endDate,
            userCreated,
            committeeCreation
            );
    }

    public void AddCommittee(string logo, string name, string description)
    {
        Committee = EventCommittee.CreateEventCommittee(logo, name, description);
    }

    public void AddDefaultMembers(CommitteeMember member)
    {
        CommitteeMembers.Add(member);
    }

    public void AddEventDates(EventDate eventDate)
    {
        EventDates.Add(eventDate);
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