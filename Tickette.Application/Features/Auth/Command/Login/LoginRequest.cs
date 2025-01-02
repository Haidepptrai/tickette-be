using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Auth.Common;

namespace Tickette.Application.Features.Auth.Command.Login;

public record LoginCommand([EmailAddress] string UserEmail, [PasswordPropertyText] string Password);

public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResultDto>
{
    private readonly IIdentityServices _identityServices;

    public LoginCommandHandler(IIdentityServices identityServices)
    {
        _identityServices = identityServices;
    }

    public async Task<LoginResultDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var (result, token, refreshToken) = await _identityServices.LoginAsync(command.UserEmail, command.Password);

        return new LoginResultDto
        {
            Succeeded = result.Succeeded,
            Token = token,
            RefreshToken = refreshToken,
            Errors = result.Succeeded ? Enumerable.Empty<string>() : result.Errors
        };
    }
}
