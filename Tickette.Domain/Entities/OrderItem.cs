using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }

    public Guid TicketId { get; private set; }

    public int Quantity { get; private set; }

    public decimal Price { get; private set; } // Price of each ticket at the time of the order

    public bool IsScanned { get; private set; }

    public DateTime? ScannedAt { get; private set; } // Later we will update of who in the event committee scanned the ticket

    public Order Order { get; private set; }

    public Ticket Ticket { get; private set; }

    public string? TicketSection { get; private set; }

    public ICollection<SeatOrder>? SeatsOrdered { get; private set; }

    private OrderItem() { }

    public OrderItem(Guid ticketId, decimal price, int quantity, string? ticketSection, ICollection<SeatOrder>? seatsOrdered)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        TicketId = ticketId;
        Price = price;
        Quantity = quantity;
        TicketSection = ticketSection;
        IsScanned = false;
        SeatsOrdered = seatsOrdered;
    }

    public static OrderItem Create(Guid ticketId, decimal price, int quantity, string? ticketSection, ICollection<SeatOrder>? seatsOrdered)
    {
        return new OrderItem(ticketId, price, quantity, ticketSection, seatsOrdered);
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

    public decimal CalculateTotalPrice()
    {
        return Price * Quantity;
    }
}
