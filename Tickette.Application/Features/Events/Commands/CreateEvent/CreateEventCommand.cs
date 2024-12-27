using Microsoft.Extensions.Logging;
using Tickette.Application.Common;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;
using Tickette.Domain.Entities;
using Tickette.Domain.ValueObjects;

namespace Tickette.Application.Features.Events.Commands.CreateEvent;

public record CreateEventCommand(
    Guid UserId,
    string Name,
    string Address,
    Guid CategoryId,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    CommitteeInformation Committee,
    TicketInformation[] TicketInformation,
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

            // Create a new event committee
            var committee = new EventCommittee()
            {
                Name = command.Committee.CommitteeName,
                Description = command.Committee.CommitteeDescription
            };

            _context.EventCommittees.Add(committee);

            // Create a new event
            //Missing committee id
            var newEvent = Event.CreateEvent(
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

            _context.Events.Add(newEvent);

            // Add ticket information
            foreach (var ticket in command.TicketInformation)
            {
                var ticketImage = await _fileStorageService.UploadFileAsync(ticket.TicketImage, "tickets");

                var ticketInfo = new Ticket(
                    eventId: newEvent.Id,
                    name: ticket.Name,
                    price: ticket.Price,
                    totalTickets: ticket.TotalTickets,
                    minTicketsPerOrder: ticket.MinTicketsPerOrder,
                    maxTicketsPerOrder: ticket.MaxTicketsPerOrder,
                    saleStartTime: ticket.SaleStartTime,
                    saleEndTime: ticket.SaleEndTime,
                    eventStartTime: ticket.EventStartTime,
                    eventEndTime: ticket.EventEndTime,
                    description: ticket.Description,
                    ticketImage: ticketImage
                );

                _context.Tickets.Add(ticketInfo);
            }

            // Add create user as admin of the event
            var admin = new CommitteeMember(command.UserId, CommitteeRole.Admin, newEvent.Id);
            _context.CommitteeMembers.Add(admin);

            await _context.SaveChangesAsync(cancellationToken);

            return newEvent.Id;
        }, "Create Event");
    }

}