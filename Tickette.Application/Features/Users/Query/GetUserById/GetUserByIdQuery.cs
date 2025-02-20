using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Users.Common;

namespace Tickette.Application.Features.Users.Query.GetUserById;

public record GetUserByIdQuery(Guid UserId, bool IsAdmin);

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, GetUserByIdResponse>
{
    private readonly IIdentityServices _identityServices;
    public GetUserByIdQueryHandler(IIdentityServices identityServices)
    {
        _identityServices = identityServices;
    }
    public async Task<GetUserByIdResponse> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await _identityServices.GetUserByIdAsync(query.UserId);

        if (!result.Succeeded)
        {
            throw new Exception("Not found user");
        }

        var data = query.IsAdmin ? result.Data.user.MapToGetUserByIdResponseForAdmin(result.Data.roles) :
            result.Data.user.MapToGetUserByIdResponseForUser();

        return data;
    }
}

