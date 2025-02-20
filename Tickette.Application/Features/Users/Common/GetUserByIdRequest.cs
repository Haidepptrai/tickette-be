namespace Tickette.Application.Features.Users.Common;

public record GetUserByIdRequest
{
    public Guid UserId { get; set; }
}