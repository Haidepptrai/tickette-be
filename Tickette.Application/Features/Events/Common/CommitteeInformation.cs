namespace Tickette.Application.Features.Events.Common;

public record CommitteeInformation
{
    public required string Name { get; set; }
    public required string Description { get; set; }
}