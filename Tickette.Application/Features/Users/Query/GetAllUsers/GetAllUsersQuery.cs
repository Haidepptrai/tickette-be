using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Models;
using Tickette.Application.Features.Users.Common;

namespace Tickette.Application.Features.Users.Query.GetAllUsers;

public record GetAllUsersQuery
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, AuthResult<IEnumerable<PreviewUserResponse>>>
{
    private readonly IIdentityServices _identityServices;

    public GetAllUsersQueryHandler(IIdentityServices identityServices)
    {
        _identityServices = identityServices;
    }

    public async Task<AuthResult<IEnumerable<PreviewUserResponse>>> Handle(GetAllUsersQuery query, CancellationToken cancellation)
    {
        // Validate pagination input
        if (query.PageNumber < 1 || query.PageSize < 1)
        {
            return AuthResult<IEnumerable<PreviewUserResponse>>.Failure(["Invalid page number or page size."]);
        }

        // Fetch users from Identity Service
        var result = await _identityServices.GetAllUsers(query.PageNumber, query.PageSize, cancellation);

        return result; // Pass through AuthResult
    }
}