using Tickette.Application.DTOs.Auth;
using Tickette.Application.Features.Events.Common.Client;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.CommitteeMembers.Common;

public static class CommitteeMemberMappers
{
    public static GetAllCommitteeMemberOfEventResponse ToCommitteeMemberResponse(this ICollection<CommitteeMember> entities, IEnumerable<RoleResponse> roles)
    {
        var members = entities.Select(entity => new CommitteeMemberDto
        {
            Id = entity.Id,
            Email = entity.User.Email!,
            Role = entity.CommitteeRole.Name,
            FullName = entity.User.FullName!
        }).ToList();

        return new GetAllCommitteeMemberOfEventResponse
        {
            Members = members,
            Roles = roles
        };
    }


}