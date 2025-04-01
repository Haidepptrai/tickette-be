namespace Tickette.Application.Features.Events.Common;

public record CommitteeRoleResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
}