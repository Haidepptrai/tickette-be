using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public class EventDate : BaseEntity
{
    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public Guid EventId { get; private set; }

    public Event Event { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public EventSeatMap? SeatMap { get; set; }

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

    public void Update(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

}