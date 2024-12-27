namespace Tickette.Application.Features.Events.Common;

public record CommitteeInformation
{
    public required string CommitteeName { get; set; }
    public required string CommitteeDescription { get; set; }
}