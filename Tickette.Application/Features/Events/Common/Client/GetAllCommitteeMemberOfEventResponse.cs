namespace Tickette.Application.Features.Events.Common.Client;

public record GetAllCommitteeMemberOfEventResponse
{
    public IEnumerable<CommitteeMemberDto> Members { get; init; }

    public IEnumerable<CommitteeRoleResponse> Roles { get; init; }
}