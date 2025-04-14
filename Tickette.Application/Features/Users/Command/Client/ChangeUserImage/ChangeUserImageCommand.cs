using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;

namespace Tickette.Application.Features.Users.Command.Client.ChangeUserImage;

public record ChangeUserImageCommand
{
    public Guid UserId { get; set; }
    public IFileUpload Image { get; init; }

    public ChangeUserImageCommand(Guid userId, IFileUpload image)
    {
        UserId = userId;
        Image = image;
    }
}

public class ChangeUserImageCommandHandler : ICommandHandler<ChangeUserImageCommand, string>
{
    private readonly IIdentityServices _identityServices;
    private readonly IFileUploadService _fileUploadServices;

    public ChangeUserImageCommandHandler(IIdentityServices identityServices, IFileUploadService fileUploadServices)
    {
        _identityServices = identityServices;
        _fileUploadServices = fileUploadServices;
    }

    public async Task<string> Handle(ChangeUserImageCommand command, CancellationToken cancellationToken)
    {
        var result = await _identityServices.GetUserByIdAsync(command.UserId);

        if (!result.Succeeded)
        {
            throw new NotFoundException("User", command.UserId);
        }

        var image = await _fileUploadServices.UploadImageAsync(command.Image, "users");

        await _identityServices.ChangeUserImageAsync(command.UserId, image);
        return image;
    }
}