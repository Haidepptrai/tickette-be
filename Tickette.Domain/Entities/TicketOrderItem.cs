using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public class TicketOrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }

    public Guid TicketId { get; private set; }

    public int Quantity { get; private set; }

    public decimal Price { get; private set; }

    public decimal GetSubtotal() => Quantity * Price;


    public TicketOrderItem(Guid ticketId, int quantity, decimal price)
    {
        TicketId = ticketId;
        Quantity = quantity;
        Price = price;
    }

    public void SetTicketOrderId(Guid orderId)
    {
        OrderId = orderId;
    }
}