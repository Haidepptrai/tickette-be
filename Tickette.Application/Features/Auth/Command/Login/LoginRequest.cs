using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Models;
using Tickette.Application.DTOs.Auth;

namespace Tickette.Application.Features.Auth.Command.Login;

public record LoginCommand([EmailAddress] string Email, [PasswordPropertyText] string Password);

public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResult<TokenRetrieval>>
{
    private readonly IIdentityServices _identityServices;

    public LoginCommandHandler(IIdentityServices identityServices)
    {
        _identityServices = identityServices;
    }

    public async Task<AuthResult<TokenRetrieval>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _identityServices.LoginAsync(command.Email, command.Password, cancellationToken);

        return result;
    }
}
