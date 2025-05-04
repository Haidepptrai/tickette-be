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

    public void MarkSeatsAsOrdered(IEnumerable<SeatOrder> reservations)
    {
        if (Tickets == null)
            return;

        // Use HashSet for performance since SeatOrder overrides Equals/GetHashCode
        var seatOrders = reservations.ToHashSet();

        foreach (var mapping in Tickets)
        {
            if (mapping.Seats == null) continue;

            foreach (var seat in mapping.Seats)
            {
                // Create a SeatOrder with the same identifying properties
                var current = new SeatOrder(seat.RowName, seat.Number);

                if (seatOrders.Contains(current))
                {
                    seat.SetIsOrdered();
                }
            }
        }
    }

}