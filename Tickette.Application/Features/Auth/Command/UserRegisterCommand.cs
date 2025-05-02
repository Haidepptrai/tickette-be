using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Models.Email;
using Tickette.Application.Factories;

namespace Tickette.Application.Features.Auth.Command;

public record UserRegisterCommand
{
    [EmailAddress]
    public string Email { get; init; }

    [PasswordPropertyText]
    public string Password { get; init; }

    public string FullName { get; init; }
}

public class UserRegisterCommandHandler : ICommandHandler<UserRegisterCommand, Guid>
{
    private readonly IIdentityServices _identityServices;
    private readonly IMessageRequestClient _messageRequestClient;

    public UserRegisterCommandHandler(IIdentityServices identityServices, IMessageRequestClient messageRequestClient)
    {
        _identityServices = identityServices;
        _messageRequestClient = messageRequestClient;
    }

    public async Task<Guid> Handle(UserRegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _identityServices.CreateUserAsync(command.FullName, command.Email, command.Password);

        if (result != Guid.Empty)
        {
            var token = await _identityServices.GenerateEmailConfirmationTokenAsync(result);

            var emailConfirmModel = new ConfirmEmailModel()
            {
                UserEmail = command.Email,
                Token = token
            };

            var wrapper = EmailWrapperFactory.Create(EmailServiceKeys.EmailConfirm, emailConfirmModel);

            //await _messageRequestClient.FireAndForgetAsync(wrapper, cancellationToken);
        }

        return result;
    }
}