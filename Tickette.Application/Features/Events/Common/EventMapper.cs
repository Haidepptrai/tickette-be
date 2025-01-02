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
        Location = entity.Address,
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
        Address = entity.Address,
        Description = entity.Description,
        Logo = entity.Logo,
        Banner = entity.Banner,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = entity.Status,
        CategoryName = entity.Category.Name,
        EventCommitteeInformation = new CommitteeInformation()
        {
            CommitteeName = entity.Committee.Name,
            CommitteeDescription = entity.Committee.Description
        },
        Tickets = entity.Tickets?.Select(ticket => new TicketDto
        {
            TicketId = ticket.Id,
            Name = ticket.Name,
            Price = ticket.Price,
            TotalTickets = ticket.TotalTickets,
            MinTicketsPerOrder = ticket.MinTicketsPerOrder,
            MaxTicketsPerOrder = ticket.MaxTicketsPerOrder,
            SaleStartTime = ticket.SaleStartTime,
            SaleEndTime = ticket.SaleEndTime,
            EventStartTime = ticket.EventStartTime,
            EventEndTime = ticket.EventEndTime,
            Description = ticket.Description,
            TicketImage = ticket.TicketImage
        }).ToList() ?? []
    };

    public static EventSeat ToEventSeat(this SeatDto seat, Guid eventId, Guid ticketId)
    {
        return EventSeat.CreateEventSeat(seat.Row, seat.Column, eventId, ticketId);
    }
}