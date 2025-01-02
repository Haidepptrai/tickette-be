using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public class EventSeat : BaseEntity
{
    public Guid EventId { get; private set; }

    public Guid TicketId { get; private set; }

    public int Row { get; private set; }

    public int Column { get; private set; }

    public Event Event { get; set; }

    public Ticket Ticket { get; set; }

    public EventSeat(int row, int column)
    {
        if (row <= 0)
        {
            throw new ArgumentException("Row must be greater than zero.", nameof(row));
        }
        if (column <= 0)
        {
            throw new ArgumentException("Column must be greater than zero.", nameof(column));
        }
        Row = row;
        Column = column;
    }
}