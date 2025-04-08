namespace Tickette.Application.Features.Users.Common;

public record PreviewUserResponse
{
    public Guid Id { get; init; }

    public string Email { get; init; }

    public string FullName { get; init; }

    public string PhoneNumber { get; init; }

    public IEnumerable<string> Roles { get; init; }
}