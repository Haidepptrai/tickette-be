using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public sealed class EventSeat : BaseEntity
{
    public Guid EventId { get; private set; }

    public Guid TicketId { get; private set; }

    public Guid EventDateId { get; private set; }

    public int Row { get; private set; }

    public int Column { get; private set; }

    public bool IsAvailable { get; private set; }

    public EventDate EventDate { get; set; }

    public Ticket Ticket { get; set; }

    private EventSeat() { }

    private EventSeat(int row, int column, EventDate eventDate, Ticket ticket)
    {
        if (row <= 0)
        {
            throw new ArgumentException("Row must be greater than zero.", nameof(row));
        }

        if (column <= 0)
        {
            throw new ArgumentException("Column must be greater than zero.", nameof(column));
        }

        EventDate = eventDate;
        Ticket = ticket;
        Row = row;
        Column = column;
        IsAvailable = true;
    }

    public static EventSeat CreateEventSeat(int row, int column, EventDate eventDate, Ticket ticket)
    {
        return new EventSeat(row, column, eventDate, ticket);
    }

    public void SetAsReserved()
    {
        IsAvailable = false;
    }
}