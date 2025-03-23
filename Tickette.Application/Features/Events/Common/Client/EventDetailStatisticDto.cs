using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Events.Common.Client;

public class EventDetailStatisticDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string LocationName { get; set; }

    public string City { get; set; }

    public string District { get; set; }

    public string Ward { get; set; }

    public string StreetAddress { get; set; }

    public required string Description { get; set; }

    public required string Banner { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public ApprovalStatus Status { get; set; }

    public required string CategoryName { get; set; }

    public required CommitteeInformation Committee { get; set; }

    public IEnumerable<EventDateDto> EventDates { get; init; }

    public string EventOwnerStripeId { get; init; }

    public IEnumerable<CommitteeMemberDto> CommitteeMembers { get; init; }
}