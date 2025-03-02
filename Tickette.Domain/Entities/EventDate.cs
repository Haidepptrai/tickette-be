using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public class EventDate : BaseEntity
{
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public Guid EventId { get; set; }

    public Event Event { get; set; }

    public ICollection<Ticket> Tickets { get; set; }

    public EventSeatMap SeatMap { get; set; }

    private EventDate() { }

    public EventDate(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static EventDate CreateEventDate(DateTime startDate, DateTime endDate)
    {
        var dateCreated = new EventDate(startDate, endDate);
        return dateCreated;
    }

    public void AddTickets(ICollection<Ticket> tickets)
    {
        Tickets = tickets;
    }

    public void AddSeatMap(EventSeatMap seatMap)
    {
        SeatMap = seatMap;
    }
}