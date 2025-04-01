namespace Tickette.Application.DTOs.Auth;

public record TokenRetrieval
{
    public required Guid UserId { get; init; }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public bool IsEmailConfirmed { get; init; }
}