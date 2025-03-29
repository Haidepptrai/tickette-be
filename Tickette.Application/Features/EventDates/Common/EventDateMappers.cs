using Tickette.Domain.Entities;

namespace Tickette.Application.Features.EventDates.Common;

public static class EventDateMappers
{
    public static EventDateForSeatMapDto MapEventDateToDto(this EventDate eventDate)
    {
        return new EventDateForSeatMapDto
        {
            Id = eventDate.Id,
            StartDate = eventDate.StartDate,
            EndDate = eventDate.EndDate,
            Tickets = eventDate.Tickets.Select(t => new TicketForSeatMapDto()
            {
                Id = t.Id,
                Name = t.Name,
                Quantity = t.RemainingTickets,
                Price = t.Price.Amount,
                Currency = t.Price.Currency,
            })
        };
    }

}