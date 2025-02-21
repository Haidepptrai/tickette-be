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
            Name = entity.Committee.Name,
            Description = entity.Committee.Description
        }
    };

    public static EventPreviewDto ToEventPreviewDto(this Event entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        LocationName = entity.LocationName,
        City = entity.City,
        District = entity.District,
        Ward = entity.Ward,
        StreetAddress = entity.StreetAddress,
        Description = entity.Description,
        Banner = entity.Banner,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        CategoryName = entity.Category.Name,
        Slug = entity.EventSlug
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
        Banner = entity.Banner,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = entity.Status,
        CategoryName = entity.Category.Name,
        EventOwnerStripeId = entity.EventOwnerStripeId,
        Committee = new CommitteeInformation()
        {
            Name = entity.Committee.Name,
            Description = entity.Committee.Description
        },
        EventDates = entity.EventDates.Select(ev => new EventDateDto()
        {
            Id = ev.Id,
            StartDate = ev.StartDate,
            EndDate = ev.EndDate,
            Tickets = ev.Tickets.Select(ticket => new TicketDto()
            {
                Id = ticket.Id,
                Name = ticket.Name,
                Currency = "USD",
                Description = ticket.Description,
                TotalTickets = ticket.TotalTickets,
                MinPerOrder = ticket.MinTicketsPerOrder,
                MaxPerOrder = ticket.MaxTicketsPerOrder,
                Price = ticket.Price,
                SaleStartTime = ticket.SaleStartTime,
                SaleEndTime = ticket.SaleEndTime,
                TicketImage = ticket.Image
            })
        })
    };
}