using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public class TicketOrder : BaseEntity
{
    public Guid EventId { get; private set; }

    public Guid UserOrderedId { get; private set; }

    public int TotalQuantity { get; private set; }

    public string BuyerEmail { get; private set; }

    public string BuyerName { get; private set; }

    public string BuyerPhone { get; private set; }

    public ICollection<TicketOrderItem> Items { get; set; }

    public decimal TotalPrice => Items.Sum(item => item.GetSubtotal());

    public decimal FinalPrice { get; private set; }

    public User UserOrdered { get; private set; }

    protected TicketOrder() { }

    public TicketOrder(Guid ticketId, Guid eventId, Guid buyerId, int quantity, string buyerEmail, string buyerName, string buyerPhone)
    {
        EventId = eventId;
        UserOrderedId = buyerId;
        TotalQuantity = quantity;
        BuyerEmail = buyerEmail;
        BuyerName = buyerName;
        BuyerPhone = buyerPhone;
    }

    public void SetFinalPrice(decimal price)
    {
        FinalPrice = price;
    }
}