namespace Tickette.Domain.Entities;

public sealed class EventSeatMap
{
    public ICollection<EventSeatMapSection>? Shapes { get; private set; }

    public ICollection<TicketSeatMapping>? Tickets { get; private set; } = new List<TicketSeatMapping>();

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
}