﻿using Tickette.Domain.Common;
using Tickette.Domain.Enums;

namespace Tickette.Domain.Entities;

public sealed class Event : BaseEntity, IAuditable
{
    public Guid CategoryId { get; private set; }

    public Guid CreatedById { get; private set; }

    public string Name { get; private set; }

    public string LocationName { get; private set; }

    public string City { get; private set; }

    public string District { get; private set; }

    public string Ward { get; private set; }

    public string StreetAddress { get; private set; }

    public string Description { get; private set; }

    public string Banner { get; private set; }

    public string EventSlug { get; private set; }

    public ApprovalStatus Status { get; private set; }

    public string? Reason { get; private set; }

    public string EventOwnerStripeId { get; private set; }

    public bool IsOffline { get; private set; }

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
        bool isOffline,
        Guid categoryId,
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
        User = userCreated;
        Committee = committee;
        CommitteeMembers = new List<CommitteeMember>();
        EventSlug = GenerateSlug(name);
        Status = ApprovalStatus.Pending;
        IsOffline = isOffline;
        Reason = null;
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
        bool isOffline,
        Guid categoryId,
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
            isOffline,
            categoryId,
            userCreated,
            committeeCreation
            );
    }

    public void UpdateEvent(
        string name,
        string locationName,
        string city,
        string district,
        string ward,
        string streetAddress,
        string description,
        string banner,
        string eventOwnerStripeId,
        bool isOffline,
        Guid categoryId,
        string committeeLogo,
        string committeeName,
        string committeeDescription
        )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Event name cannot be empty.", nameof(name));

        if (isOffline)
        {
            if (string.IsNullOrWhiteSpace(locationName))
                throw new ArgumentException("Location name cannot be empty for offline events.", nameof(locationName));

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be empty for offline events.", nameof(city));

            if (string.IsNullOrWhiteSpace(district))
                throw new ArgumentException("District cannot be empty for offline events.", nameof(district));

            if (string.IsNullOrWhiteSpace(ward))
                throw new ArgumentException("Ward cannot be empty for offline events.", nameof(ward));

            if (string.IsNullOrWhiteSpace(streetAddress))
                throw new ArgumentException("Street address cannot be empty for offline events.", nameof(streetAddress));
        }

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        if (string.IsNullOrWhiteSpace(banner))
            throw new ArgumentException("Banner URL cannot be empty.", nameof(banner));

        if (string.IsNullOrWhiteSpace(eventOwnerStripeId))
            throw new ArgumentException("Stripe account ID cannot be empty.", nameof(eventOwnerStripeId));

        // Update base event details
        Name = name;
        LocationName = locationName;
        City = city;
        District = district;
        Ward = ward;
        StreetAddress = streetAddress;
        Description = description;
        Banner = banner;
        EventOwnerStripeId = eventOwnerStripeId;
        IsOffline = isOffline;
        CategoryId = categoryId;

        // Update committee
        this.Committee.UpdateCommittee(committeeLogo, committeeName, committeeDescription);

        // Regenerate slug if the name has changed
        EventSlug = GenerateSlug(name);
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

    public void ChangeStatus(ApprovalStatus status, string? reason = null)
    {
        //if (status == ApprovalStatus.Approved && Status == ApprovalStatus.Approved)
        //    throw new InvalidOperationException("Event status cannot be updated once it is approved.");

        Status = status;
        Reason = reason;
    }

    public void UpdateEventDate(Guid eventDateId, DateTime startDate, DateTime endDate)
    {
        var eventDate = EventDates.FirstOrDefault(ed => ed.Id == eventDateId);
        if (eventDate == null)
            throw new InvalidOperationException("Event date not found.");
        eventDate.Update(startDate, endDate);
    }

    public void UpdateEventDateTicket(
        Guid eventDateId,
        Guid ticketId,
        string name,
        decimal amount,
        string currency,
        int totalTickets,
        int minPerOrder,
        int maxPerOrder,
        DateTime saleStartTime,
        DateTime saleEndTime,
        string description,
        string? ticketImageUrl = null)
    {
        var eventDate = EventDates.FirstOrDefault(ed => ed.Id == eventDateId);
        if (eventDate == null)
            throw new InvalidOperationException("Event date not found.");

        var ticket = eventDate.Tickets.FirstOrDefault(t => t.Id == ticketId);
        if (ticket == null)
            throw new InvalidOperationException("Ticket not found.");

        ticket.Update(
            name: name,
            amount: amount,
            currency: currency,
            totalTickets: totalTickets,
            minPerOrder: minPerOrder,
            maxPerOrder: maxPerOrder,
            saleStartTime: saleStartTime,
            saleEndTime: saleEndTime,
            description: description,
            ticketImageUrl: ticketImageUrl
        );
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