using System.Text.Json.Serialization;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;

namespace Tickette.Application.Features.Auth.Command.Logout;

public record LogoutCommand
{
    [JsonIgnore]
    public Guid UserId { get; set; }

    public string RefreshToken { get; init; }
}

public class LogoutCommandHandler : ICommandHandler<LogoutCommand, bool>
{
    private readonly IIdentityServices _identityServices;

    public LogoutCommandHandler(IIdentityServices identityServices)
    {
        _identityServices = identityServices;
    }

    public async Task<bool> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var result = await _identityServices.LogoutAsync(command.UserId, command.RefreshToken, cancellationToken);
        return result;
    }
}