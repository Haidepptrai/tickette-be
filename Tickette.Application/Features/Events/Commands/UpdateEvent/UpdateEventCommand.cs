using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;
using Tickette.Application.Factories;
using Tickette.Application.Features.Events.Common;
using Tickette.Domain.Entities;
using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Events.Commands.UpdateEvent;

public record UpdateEventCommand(
    Guid Id,
    Guid UserId,
    string Name,
    string LocationName,
    string City,
    string District,
    string Ward,
    string StreetAddress,
    Guid CategoryId,
    string Description,
    IFileUpload? CommitteeLogo,
    string? CommitteeLogoUrl,
    string CommitteeName,
    string CommitteeDescription,
    bool IsOffline,
    EventDateInputForUpdate[] EventDatesInformation,
    IFileUpload? BannerFile,
    string? BannerUrl,
    string EventOwnerStripeId
);


public class UpdateEventCommandCommandHandler : ICommandHandler<UpdateEventCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileUploadService _fileUploadService;
    private readonly IIdentityServices _identityServices;

    public UpdateEventCommandCommandHandler(IApplicationDbContext context, IFileUploadService fileUploadService, IIdentityServices identityServices)
    {
        _context = context;
        _fileUploadService = fileUploadService;
        _identityServices = identityServices;
    }

    public async Task<Guid> Handle(UpdateEventCommand command, CancellationToken cancellationToken)
    {
        var eventToUpdate = await _context.Events
            .Include(e => e.Committee)
            .Include(e => e.EventDates)
            .ThenInclude(ed => ed.Tickets)
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

        if (eventToUpdate == null)
            throw new NotFoundException("Event", command.Id);

        if (eventToUpdate.Status is ApprovalStatus.Approved or ApprovalStatus.Pending)
            throw new InvalidOperationException("Event cannot be updated after approval or while wait for review.");

        var bannerUrl = command.BannerFile != null
            ? await _fileUploadService.UploadImageAsync(command.BannerFile, "events")
            : command.BannerUrl;


        var committeeUrl = command.CommitteeLogo != null
            ? await _fileUploadService.UploadImageAsync(command.CommitteeLogo, "committees")
            : command.CommitteeLogoUrl;

        eventToUpdate.UpdateEvent(
            name: command.Name,
            locationName: command.LocationName,
            city: command.City,
            district: command.District,
            ward: command.Ward,
            streetAddress: command.StreetAddress,
            description: command.Description,
            categoryId: command.CategoryId,
            banner: bannerUrl!,
            eventOwnerStripeId: command.EventOwnerStripeId,
            isOffline: command.IsOffline,
            committeeLogo: committeeUrl!,
            committeeName: command.CommitteeName,
            committeeDescription: command.CommitteeDescription
            );

        var existingEventDates = eventToUpdate.EventDates.ToList();

        foreach (var inputDate in command.EventDatesInformation)
        {
            var existingDate = existingEventDates.FirstOrDefault(ed => ed.Id == inputDate.Id);

            if (existingDate != null)
            {
                eventToUpdate.UpdateEventDate(existingDate.Id, inputDate.StartDate, inputDate.EndDate);

                var existingTickets = existingDate.Tickets.ToList();

                foreach (var inputTicket in inputDate.Tickets)
                {
                    var existingTicket = existingTickets.FirstOrDefault(t => t.Id == inputTicket.Id);

                    if (existingTicket != null)
                    {
                        var ticketImageUrl = inputTicket.TicketImage != null
                            ? await _fileUploadService.UploadImageAsync(inputTicket.TicketImage, "tickets")
                            : null;

                        eventToUpdate.UpdateEventDateTicket(
                            eventDateId: existingDate.Id,
                            existingTicket.Id,
                            inputTicket.Name,
                            inputTicket.Amount,
                            inputTicket.Currency,
                            inputTicket.TotalTickets,
                            inputTicket.MinPerOrder,
                            inputTicket.MaxPerOrder,
                            inputTicket.SaleStartTime,
                            inputTicket.SaleEndTime,
                            inputTicket.Description,
                            ticketImageUrl
                        );
                    }
                    else
                    {
                        var newTicket = await TicketFactory.CreateTicketAsync(
                            existingDate,
                            inputTicket.Name,
                            inputTicket.Amount,
                            inputTicket.Currency,
                            inputTicket.TotalTickets,
                            inputTicket.MinPerOrder,
                            inputTicket.MaxPerOrder,
                            inputTicket.SaleStartTime,
                            inputTicket.SaleEndTime,
                            inputTicket.Description,
                            inputTicket.TicketImage,
                            _fileUploadService
                        );

                        existingDate.AddTickets(new[] { newTicket });
                    }
                }

                // Handle deleted tickets
                var inputTicketIds = inputDate.Tickets.Select(t => t.Id).ToHashSet();
                var ticketsToRemove = existingTickets.Where(t => !inputTicketIds.Contains(t.Id)).ToList();
                foreach (var ticket in ticketsToRemove)
                {
                    ticket.SoftDeleteEntity();
                    _context.Tickets.Update(ticket);
                }
            }
            else
            {
                var newDate = EventDate.CreateEventDate(inputDate.StartDate, inputDate.EndDate);

                var newTickets = await Task.WhenAll(inputDate.Tickets.Select(ticket =>
                    TicketFactory.CreateTicketAsync(
                        newDate,
                        ticket.Name,
                        ticket.Amount,
                        ticket.Currency,
                        ticket.TotalTickets,
                        ticket.MinPerOrder,
                        ticket.MaxPerOrder,
                        ticket.SaleStartTime,
                        ticket.SaleEndTime,
                        ticket.Description,
                        ticket.TicketImage,
                        _fileUploadService
                    )));

                newDate.AddTickets(newTickets);
                eventToUpdate.AddEventDates(newDate);
            }
        }

        // Delete removed event dates
        var inputDateIds = command.EventDatesInformation.Select(ed => ed.Id).ToHashSet();
        var eventDatesToRemove = existingEventDates.Where(ed => !inputDateIds.Contains(ed.Id)).ToList();
        foreach (var ed in eventDatesToRemove)
        {
            ed.SoftDeleteEntity();
            _context.EventDates.Update(ed);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}