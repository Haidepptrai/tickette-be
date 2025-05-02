namespace Tickette.Domain.Entities;

public sealed class EventSeatMap
{
    public ICollection<EventSeatMapSection>? Shapes { get; set; }

    public ICollection<TicketSeatMapping>? Tickets { get; set; }

    public EventSeatMap() { }

    private EventSeatMap(ICollection<EventSeatMapSection>? shapes, ICollection<TicketSeatMapping>? tickets)
    {
        Shapes = shapes;
        Tickets = tickets;
    }

    public static EventSeatMap CreateEventSeatMap(ICollection<EventSeatMapSection>? sections, ICollection<TicketSeatMapping>? tickets)
    {
        return new EventSeatMap(sections, tickets);
    }

    public IEnumerable<EventSeat> GetAllSeats()
    {
        return Tickets?.Where(t => t.Seats != null).SelectMany(t => t.Seats!) ?? [];
    }
}