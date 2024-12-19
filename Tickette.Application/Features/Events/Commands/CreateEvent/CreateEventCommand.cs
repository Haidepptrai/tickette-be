using Microsoft.Extensions.Logging;
using Tickette.Application.Common;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Events.Commands.CreateEvent;

public record CreateEventCommand(
    string Name,
    string Address,
    Guid CategoryId,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    CommitteeInformation Committee,
    IFileUpload LogoFile,
    IFileUpload BannerFile
);

public class CreateEventCommandHandler : BaseHandler<CreateEventCommandHandler>, ICommandHandler<CreateEventCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public CreateEventCommandHandler(IApplicationDbContext context, ILogger<CreateEventCommandHandler> logger, IFileStorageService fileStorageService) : base(logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<Guid> Handle(CreateEventCommand command, CancellationToken cancellationToken)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            // Upload Logo and Banner to S3
            string logoUrl = await _fileStorageService.UploadFileAsync(command.LogoFile, "logos");
            string bannerUrl = await _fileStorageService.UploadFileAsync(command.BannerFile, "banners");

            var committee = new EventCommittee()
            {
                Name = command.Committee.CommitteeName,
                Description = command.Committee.ComitteeDescription
            };

            _context.EventCommittees.Add(committee);

            var entity = Event.CreateEvent(
                name: command.Name,
                address: command.Address,
                categoryId: command.CategoryId,
                description: command.Description,
                logo: logoUrl,
                banner: bannerUrl,
                startDate: command.StartDate,
                endDate: command.EndDate,
                committee: committee
            );

            _context.Events.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }, "Create Event");
    }

}