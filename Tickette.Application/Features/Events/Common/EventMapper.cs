using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Events.Common;

public static class EventMapper
{
    public static EventListDto ToEventListDto(this Event entity) => new()
    {
        Name = entity.Name,
        Description = entity.Description,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        LocationName = entity.LocationName,
        City = entity.City,
        District = entity.District,
        Ward = entity.Ward,
        StreetAddress = entity.StreetAddress,
        ImageUrl = entity.Banner,
        Category = entity.Category.Name,
        Committee = new CommitteeInformation()
        {
            CommitteeName = entity.Committee.Name,
            CommitteeDescription = entity.Committee.Description
        }
    };

    public static EventDetailDto ToEventDetailDto(this Event entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        LocationName = entity.LocationName,
        City = entity.City,
        District = entity.District,
        Ward = entity.Ward,
        StreetAddress = entity.StreetAddress,
        Description = entity.Description,
        Logo = entity.Logo,
        Banner = entity.Banner,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = entity.Status,
        CategoryName = entity.Category.Name,
        CommitteeInformation = new CommitteeInformation()
        {
            CommitteeName = entity.Committee.Name,
            CommitteeDescription = entity.Committee.Description
        },
        EventDates = entity.EventDates.Select(ev => new EventDateDto()
        {
            StartDate = ev.StartDate,
            EndDate = ev.EndDate,
            Tickets = ev.Tickets.Select(ticket => new TicketDto()
            {
                TicketId = ticket.Id,
                Name = ticket.Name,
                Currency = "USD",
                Description = ticket.Description,
                TotalTickets = ticket.TotalTickets,
                MinTicketsPerOrder = ticket.MinTicketsPerOrder,
                MaxTicketsPerOrder = ticket.MaxTicketsPerOrder,
                Price = ticket.Price,
                SaleStartTime = ticket.SaleStartTime,
                SaleEndTime = ticket.SaleEndTime,
                TicketImage = ticket.Image
            })
        })
    };
}