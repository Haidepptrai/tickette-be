namespace Tickette.Domain.Entities;

public sealed class TicketSeatMapping
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public ICollection<EventSeat> Seats { get; private set; }

    public TicketSeatMapping(Guid id, string name, ICollection<EventSeat> seats)
    {
        Id = id;
        Name = name;
        Seats = seats;
    }

    public static TicketSeatMapping CreateEventSeatMap(Guid id, string name, ICollection<EventSeat> seats)
    {
        return new TicketSeatMapping(id, name, seats);
    }
}
