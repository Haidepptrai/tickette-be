using System.Text.Json.Serialization;
using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Events.Common;

public class EventDetailDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Address { get; set; }

    public required string Description { get; set; }

    public required string Logo { get; set; }

    public required string Banner { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ApprovalStatus Status { get; set; }

    public required string CategoryName { get; set; }

    public required CommitteeInformation EventCommitteeInformation { get; set; }

    public List<TicketDto> Tickets { get; set; } = [];
}