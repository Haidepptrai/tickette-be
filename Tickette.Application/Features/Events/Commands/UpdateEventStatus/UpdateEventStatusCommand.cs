using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Events.Commands.UpdateEventStatus;

public record UpdateEventStatusCommand(Guid EventId, ApprovalStatus Status);

public class UpdateEventStatusHandler : ICommandHandler<UpdateEventStatusCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public UpdateEventStatusHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(UpdateEventStatusCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var eventToUpdate = await _context.Events.SingleOrDefaultAsync(ev => ev.Id == command.EventId, cancellationToken);

            if (eventToUpdate == null)
                throw new KeyNotFoundException($"Event with ID {command.EventId} was not found.");

            eventToUpdate.Status = command.Status;

            await _context.SaveChangesAsync(cancellationToken);

            return command.EventId;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while updating the status of the event with ID {command.EventId}.", ex);
        }
    }
}