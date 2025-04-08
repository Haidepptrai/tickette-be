using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Users.Common;

namespace Tickette.Application.Features.Users.Query.Admin.AdminGetUserById;

public record AdminGetUserByIdQuery(Guid UserId);

public class GetUserByIdQueryHandler : IQueryHandler<AdminGetUserByIdQuery, GetUserByIdResponse>
{
    private readonly IIdentityServices _identityServices;

    public GetUserByIdQueryHandler(IIdentityServices identityServices)
    {
        _identityServices = identityServices;
    }

    public async Task<GetUserByIdResponse> Handle(AdminGetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await _identityServices.GetUserByIdWithRolesAsync(query.UserId);

        return result;
    }
}

