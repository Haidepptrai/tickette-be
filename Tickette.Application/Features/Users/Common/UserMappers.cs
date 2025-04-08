using Tickette.Application.DTOs.Auth;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Users.Common;

public static class UserMappers
{
    public static PreviewUserResponse MapToPreviewUserResponse(this User user, IEnumerable<string> role)
    {
        return new PreviewUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Roles = role
        };
    }

    public static GetUserByIdResponse MapToGetUserByIdResponseForAdmin(this User user, IEnumerable<string> roles, IEnumerable<RoleResponse> systemRoles)
    {
        return new GetUserByIdResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Roles = roles,
            SystemRoles = systemRoles
        };
    }

    public static GetUserByIdResponse MapToGetUserByIdResponseForUser(this User user)
    {
        return new GetUserByIdResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber
        };
    }

}