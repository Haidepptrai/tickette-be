using System.Text.Json.Serialization;
using Tickette.Application.Features.Events.Common.Client;
using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Events.Common;

public class EventDetailDto
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public bool IsOffline { get; init; }

    public string LocationName { get; init; }

    public string City { get; init; }

    public string District { get; init; }

    public string Ward { get; init; }

    public string StreetAddress { get; init; }

    public required string Description { get; init; }

    public required string Banner { get; init; }

    public required Guid CategoryId { get; init; }

    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public string CommitteeName { get; init; }

    public string CommitteeDescription { get; init; }

    public string CommitteeLogo { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ApprovalStatus Status { get; init; }

    public IEnumerable<EventDateDto> EventDates { get; init; }

    public string EventOwnerStripeId { get; init; }

    public IEnumerable<CategoryDto>? Categories { get; init; }

    public CommitteeMemberDto? UserInEventInfo { get; init; }

    public string? Reason { get; init; }
}