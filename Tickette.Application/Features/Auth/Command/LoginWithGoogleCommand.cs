using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.DTOs.Auth;

namespace Tickette.Application.Features.Auth.Command;

public record LoginWithGoogleCommand
{
    public string Email { get; init; }
    public string Name { get; init; }
    public string Image { get; init; }
    public string Provider { get; init; } // Optional: This could be expanded for other providers
}

public class LoginWithGoogleCommandHandler : ICommandHandler<LoginWithGoogleCommand, TokenRetrieval>
{
    private readonly IIdentityServices _identityServices;

    public LoginWithGoogleCommandHandler(IIdentityServices identityServices)
    {
        _identityServices = identityServices;
    }

    public async Task<TokenRetrieval> Handle(LoginWithGoogleCommand command, CancellationToken cancellationToken)
    {
        var result = await _identityServices.SyncGoogleUserAsync(command);
        return result;
    }
}