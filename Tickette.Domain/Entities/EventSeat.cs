using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public sealed class EventSeat : BaseEntity
{
    public Guid EventId { get; private set; }

    public Guid TicketId { get; private set; }

    public int Row { get; private set; }

    public int Column { get; private set; }

    public bool IsAvailable { get; private set; }

    public Event Event { get; set; }

    public Ticket Ticket { get; set; }

    private EventSeat(int row, int column, Guid eventId, Guid ticketId)
    {
        if (row <= 0)
        {
            throw new ArgumentException("Row must be greater than zero.", nameof(row));
        }

        if (column <= 0)
        {
            throw new ArgumentException("Column must be greater than zero.", nameof(column));
        }

        EventId = eventId;
        TicketId = ticketId;
        Row = row;
        Column = column;
        IsAvailable = true;
    }

    public static EventSeat CreateEventSeat(int row, int column, Guid eventId, Guid ticketId)
    {
        return new EventSeat(row, column, eventId, ticketId);
    }

    public void OrderSeat(int row, int column)
    {
        if (row <= 0)
        {
            throw new ArgumentException("Row must be greater than zero.", nameof(row));
        }

        if (column <= 0)
        {
            throw new ArgumentException("Column must be greater than zero.", nameof(column));
        }

        if (row != Row || column != Column)
        {
            throw new InvalidOperationException("Seat is not available.");
        }

        IsAvailable = false;
    }
}