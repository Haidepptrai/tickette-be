using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;
using Tickette.Domain.Common;
using Tickette.Domain.Entities;

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
    ICollection<SeatDto>? Seats,
    TicketInformation[] TicketInformation,
    IFileUpload LogoFile,
    IFileUpload BannerFile
);

public class CreateEventCommandHandler : ICommandHandler<CreateEventCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public CreateEventCommandHandler(IApplicationDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<Guid> Handle(CreateEventCommand command, CancellationToken cancellationToken)
    {
        // Upload Logo and Banner to S3
        var logoUrl = await _fileStorageService.UploadFileAsync(command.LogoFile, "logos");
        var bannerUrl = await _fileStorageService.UploadFileAsync(command.BannerFile, "banners");

        // Create a new event committee
        var committee = new EventCommittee()
        {
            Name = command.Committee.CommitteeName,
            Description = command.Committee.CommitteeDescription
        };

        _context.EventCommittees.Add(committee);

        // Create a new event
        var newEvent = Event.CreateEvent(
            name: command.Name,
            address: command.Address,
            categoryId: command.CategoryId,
            description: command.Description,
            logo: logoUrl,
            banner: bannerUrl,
            startDate: command.StartDate,
            endDate: command.EndDate,
            committee: committee,
            members: new List<CommitteeMember>(),
            seats: new List<EventSeat>()
        );

        _context.Events.Add(newEvent);

        if (command.Seats != null)
        {
            var seats = command.Seats.Select(s => s.ToEventSeat(newEvent.Id, s.TicketId)).ToList();
            newEvent.AddSeats(seats);
        }

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

        var committeeRole = await _context.CommitteeRoles
            .SingleOrDefaultAsync(r => r.Name == Constant.COMMITTEE_MEMBER_ROLES.EventOwner, cancellationToken);

        if (committeeRole == null)
        {
            throw new Exception($"Role '{Constant.COMMITTEE_MEMBER_ROLES.EventOwner}' not found.");
        }

        // Add create user as admin of the event
        var admin = new CommitteeMember(command.UserId, committeeRole.Id, newEvent.Id);
        newEvent.AddDefaultMembers(admin);

        await _context.SaveChangesAsync(cancellationToken);

        return newEvent.Id;
    }
}