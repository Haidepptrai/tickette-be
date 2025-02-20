namespace Tickette.Application.DTOs.Auth;

public record RoleResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; }
}