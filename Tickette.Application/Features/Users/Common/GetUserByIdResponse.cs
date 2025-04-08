using Tickette.Application.DTOs.Auth;

namespace Tickette.Application.Features.Users.Common;

public record GetUserByIdResponse
{
    public Guid Id { get; init; }

    public string Email { get; init; }

    public string FullName { get; init; }

    public string PhoneNumber { get; init; }

    public IEnumerable<string> Roles { get; init; }

    public IEnumerable<RoleResponse> SystemRoles { get; init; }
}