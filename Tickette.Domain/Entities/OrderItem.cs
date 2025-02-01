using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }

    public Guid TicketId { get; private set; }

    public int Quantity { get; private set; }

    public long Price { get; private set; } // Price of each ticket at the time of the order

    public bool IsScanned { get; private set; }

    public DateTime? ScannedAt { get; private set; } // Later we will update of who in the event committee scanned the ticket

    public Order Order { get; private set; }

    public Ticket Ticket { get; private set; }

    public ICollection<EventSeat> Seats = new List<EventSeat>();


    private OrderItem() { }

    public OrderItem(Guid ticketId, long price, int quantity, List<EventSeat>? seats)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        TicketId = ticketId;
        Price = price;
        Quantity = quantity;
        Seats = seats ?? [];
        IsScanned = false;
    }

    public static OrderItem Create(Guid ticketId, long price, int quantity, List<EventSeat>? seats)
    {
        return new OrderItem(ticketId, price, quantity, seats);
    }

    public void SetAsScanned()
    {
        IsScanned = true;
        ScannedAt = DateTime.UtcNow;
    }

    public void AddQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        Quantity += quantity;
    }

    public long CalculateTotalPrice()
    {
        return Price * Quantity;
    }
}
