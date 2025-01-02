using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }

    public Guid TicketId { get; private set; }

    public int Quantity { get; private set; }

    public OrderItem(Guid ticketId, int quantity)
    {
        TicketId = ticketId;
        Quantity = quantity;
    }

    public void SetTicketOrderId(Guid orderId)
    {
        OrderId = orderId;
    }
}