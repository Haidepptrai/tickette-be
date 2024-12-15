using Microsoft.Extensions.Logging;
using Tickette.Application.Common;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Events.Commands.CreateEvent;

public record CreateEventCommand
{
    public string Name { get; set; }

    public string Address { get; set; }

    public Guid CategoryId { get; set; }

    public string Description { get; set; }

    public string Logo { get; set; }

    public string Banner { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public CommitteeInformation Committee { get; set; }
}

public class CreateEventCommandHandler : BaseHandler<CreateEventCommandHandler>, ICommandHandler<CreateEventCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateEventCommandHandler(IApplicationDbContext context, ILogger<CreateEventCommandHandler> logger) : base(logger)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var committee = new EventCommittee()
            {
                Name = request.Committee.Name,
                Description = request.Committee.Description
            };

            _context.EventCommittees.Add(committee);

            var entity = Event.CreateEvent(
                name: request.Name,
                address: request.Address,
                categoryId: request.CategoryId,
                description: request.Description,
                logo: request.Logo,
                banner: request.Banner,
                startDate: request.StartDate,
                endDate: request.EndDate,
                committee: committee
            );

            _context.Events.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }, "Create Event");
    }

}