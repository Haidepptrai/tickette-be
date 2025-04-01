using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;

namespace Tickette.Application.Features.Auth.Command.ConfirmEmail;

public record ConfirmEmailCommand
{
    public string Token { get; init; }

    public string UserEmail { get; init; }
}

public class ConfirmEmailCommandHandler : ICommandHandler<ConfirmEmailCommand, bool>
{
    private readonly IIdentityServices _identityServices;

    public ConfirmEmailCommandHandler(IIdentityServices identityServices)
    {
        _identityServices = identityServices;
    }

    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        return await _identityServices.ConfirmEmailAsync(request.Token, request.UserEmail);
    }
}