using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public class Order : BaseEntity
{
    public Guid EventId { get; private set; }

    public Guid UserOrderedId { get; private set; }

    public int TotalQuantity { get; private set; }

    public string BuyerEmail { get; private set; }

    public string BuyerName { get; private set; }

    public string BuyerPhone { get; private set; }

    public ICollection<OrderItem> Items { get; set; }

    public decimal TotalPrice { get; private set; }

    public decimal FinalPrice { get; private set; }

    public User UserOrdered { get; private set; }

    protected Order() { }

    public Order(Guid eventId, Guid buyerId, string buyerEmail, string buyerName, string buyerPhone)
    {
        EventId = eventId;
        UserOrderedId = buyerId;
        BuyerEmail = buyerEmail;
        BuyerName = buyerName;
        BuyerPhone = buyerPhone;
    }

    public void SetFinalPrice(decimal price)
    {
        FinalPrice = price;
    }

    // Count the total quantity of the order
    public void CountTotalQuantity(int quantity)
    {
        TotalQuantity = quantity;
    }
}