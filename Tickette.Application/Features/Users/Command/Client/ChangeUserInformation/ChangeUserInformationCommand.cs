using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;
using Tickette.Domain.Common;

namespace Tickette.Application.Features.Users.Command.Client.ChangeUserInformation;

public record ChangeUserInformationCommand
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
}

public class ChangeUserInformationCommandHandler : ICommandHandler<ChangeUserInformationCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public ChangeUserInformationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ChangeUserInformationCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == command.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", command.UserId);
        }

        user.FullName = command.FullName;
        user.PhoneNumber = command.Phone;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}