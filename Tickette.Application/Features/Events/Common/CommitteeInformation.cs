namespace Tickette.Application.Features.Events.Common;

public record CommitteeInformation
{
    public required string Logo { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
}