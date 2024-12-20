using Tickette.Application.Features.CommitteeMembers.Commond;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.CommitteeMembers.Common;

public static class CommitteeMemberMappers
{
    public static CommitteeMemberDto ToCommitteeMemberDto(this CommitteeMember entity)
    {
        return new CommitteeMemberDto
        {
            Id = entity.Id,
            Email = entity.User.Email!,
            Role = entity.Role.Name,
        };
    }

}