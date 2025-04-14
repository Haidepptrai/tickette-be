using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Factories;
using Tickette.Application.Features.Events.Common;
using Tickette.Domain.Common;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Events.Commands.CreateEvent;

public record CreateEventCommand(
    Guid UserId,
    string Name,
    string LocationName,
    string City,
    string District,
    string Ward,
    string StreetAddress,
    Guid CategoryId,
    string Description,
    IFileUpload CommitteeLogo,
    string CommitteeName,
    string CommitteeDescription,
    bool IsOffline,
    EventDateInput[] EventDatesInformation,
    IFileUpload BannerFile,
    string EventOwnerStripeId
);


public class CreateEventCommandHandler : ICommandHandler<CreateEventCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileUploadService _fileUploadService;
    private readonly IIdentityServices _identityServices;

    public CreateEventCommandHandler(IApplicationDbContext context, IFileUploadService fileUploadService, IIdentityServices identityServices)
    {
        _context = context;
        _fileUploadService = fileUploadService;
        _identityServices = identityServices;
    }

    public async Task<Guid> Handle(CreateEventCommand command, CancellationToken cancellationToken)
    {
        var userCreatedResult = await _identityServices.GetUserByIdAsync(command.UserId);

        var bannerUrl = await _fileUploadService.UploadImageAsync(command.BannerFile, "banners");

        var committeeUrl = await _fileUploadService.UploadImageAsync(command.CommitteeLogo, "committees");
        var committee = EventCommittee.CreateEventCommittee(committeeUrl, command.CommitteeName, command.CommitteeDescription);
        var newEventCreated = Event.CreateEvent(
            name: command.Name,
            locationName: command.LocationName,
            city: command.City,
            district: command.District,
            ward: command.Ward,
            streetAddress: command.StreetAddress,
            description: command.Description,
            categoryId: command.CategoryId,
            userCreated: userCreatedResult.Data.user,
            banner: bannerUrl,
            committee: committee,
            eventOwnerStripeId: command.EventOwnerStripeId,
            isOffline: command.IsOffline);

        // Add ticket information
        foreach (var eventDate in command.EventDatesInformation)
        {
            // First create all tickets for each event date
            var eventDateEntity = EventDate.CreateEventDate(eventDate.StartDate, eventDate.EndDate);

            var tickets = await Task.WhenAll(eventDate.Tickets.Select(ticket => TicketFactory.CreateTicketAsync(
                eventDate: eventDateEntity,
                name: ticket.Name,
                amount: ticket.Amount,
                currency: ticket.Currency,
                totalTickets: ticket.TotalTickets,
                minTicketsPerOrder: ticket.MinPerOrder,
                maxTicketsPerOrder: ticket.MaxPerOrder,
                saleStartTime: ticket.SaleStartTime,
                saleEndTime: ticket.SaleEndTime,
                description: ticket.Description,
                ticketImageFile: ticket.TicketImage,
                fileUploadService: _fileUploadService
            )));

            eventDateEntity.AddTickets(tickets);

            newEventCreated.AddEventDates(eventDateEntity);
        }

        var committeeRoleAdmin = _context.CommitteeRoles.FirstOrDefault(x => x.Name == Constant.COMMITTEE_MEMBER_ROLES.Admin);

        if (committeeRoleAdmin == null)
        {
            throw new Exception("An error has occurred while assign you as admin. Please contact support if this error continue");
        }

        var admin = CommitteeMember.Create(userCreatedResult.Data.user, committeeRoleAdmin, newEventCreated);

        newEventCreated.AddDefaultMembers(admin);

        _context.Events.Add(newEventCreated);
        await _context.SaveChangesAsync(cancellationToken);

        return newEventCreated.Id;
    }
}