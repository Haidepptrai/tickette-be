using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Users.Common;

namespace Tickette.Application.Features.Users.Query.GetUserById;

public record GetUserByIdQuery(Guid UserId);

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, GetUserByIdResponse>
{
    private readonly IIdentityServices _identityServices;
    public GetUserByIdQueryHandler(IIdentityServices identityServices)
    {
        _identityServices = identityServices;
    }
    public async Task<GetUserByIdResponse> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await _identityServices.GetUserByIdAsync(query.UserId);

        if (!user.Succeeded)
        {
            throw new Exception("Not found user");
        }

        if (user.Data == null)
        {
            throw new Exception("Not found user");
        }

        return new GetUserByIdResponse
        {
            Id = user.Data.Id,
            Email = user.Data.Email,
            FullName = user.Data.FullName,
            PhoneNumber = user.Data.PhoneNumber
        };
    }
}

