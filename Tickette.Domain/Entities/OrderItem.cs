using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid TicketId { get; private set; }
    public int Quantity { get; private set; }
    public decimal Price { get; private set; } // Price of the ticket at the time of the order

    protected OrderItem() { }

    public OrderItem(Guid ticketId, decimal price, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        TicketId = ticketId;
        Price = price;
        Quantity = quantity;
    }

    public void SetTicketOrderId(Guid orderId)
    {
        OrderId = orderId;
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
