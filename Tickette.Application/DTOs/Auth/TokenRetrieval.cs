namespace Tickette.Application.DTOs.Auth;

public record TokenRetrieval
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}