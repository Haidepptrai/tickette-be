namespace Tickette.Application.Features.Events.Common.Client;

public record CommitteeMemberDto
{
    public Guid UserId { get; init; }

    public string FullName { get; init; }

    public string Email { get; init; }

    public string Role { get; init; }
}