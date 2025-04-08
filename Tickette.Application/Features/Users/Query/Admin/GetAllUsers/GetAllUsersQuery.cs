using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Users.Common;
using Tickette.Application.Wrappers;

namespace Tickette.Application.Features.Users.Query.Admin.GetAllUsers;

public record GetAllUsersQuery
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public string? Search { get; init; }
}

public class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, PagedResult<PreviewUserResponse>>
{
    private readonly IIdentityServices _identityServices;

    public GetAllUsersQueryHandler(IIdentityServices identityServices)
    {
        _identityServices = identityServices;
    }

    public async Task<PagedResult<PreviewUserResponse>> Handle(GetAllUsersQuery query, CancellationToken cancellation)
    {
        var result = await _identityServices.GetAllUsers(query.PageNumber, query.PageSize, query.Search, cancellation);

        return result;
    }
}