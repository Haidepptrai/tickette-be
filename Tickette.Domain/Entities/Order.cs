using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public sealed class Order : BaseEntity
{
    public Guid EventId { get; private set; }

    public Guid UserOrderedId { get; private set; }

    public Guid? CouponId { get; private set; }

    public string BuyerEmail { get; private set; }

    public string BuyerName { get; private set; }

    public string BuyerPhone { get; private set; }

    public long OriginalPrice { get; private set; }

    public long TotalPrice { get; private set; }

    public long DiscountAmount { get; private set; }

    public int TotalQuantity { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Event Event { get; private set; }

    public User UserOrdered { get; private set; }

    private Order() { }

    private Order(Guid eventId, Guid buyerId, string buyerEmail, string buyerName, string buyerPhone)
    {
        EventId = eventId;
        UserOrderedId = buyerId;
        BuyerEmail = buyerEmail;
        BuyerName = buyerName;
        BuyerPhone = buyerPhone;
    }


    public static Order CreateOrder(Guid eventId, Guid buyerId, string buyerEmail, string buyerName, string buyerPhone)
    {
        return new Order(eventId, buyerId, buyerEmail, buyerName, buyerPhone);
    }

    public void AddOrderItem(Guid ticketId, long ticketPrice, int quantity, List<EventSeat>? seats)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        var existingItem = _items.SingleOrDefault(x => x.TicketId == ticketId);

        if (existingItem is not null)
        {
            existingItem.AddQuantity(quantity);
        }
        else
        {
            _items.Add(new OrderItem(ticketId, ticketPrice, quantity, seats));
        }

        CalculateTotals();
    }

    private void CalculateTotals()
    {
        OriginalPrice = _items.Sum(item => item.CalculateTotalPrice());
        TotalPrice = OriginalPrice;
        TotalQuantity = _items.Sum(item => item.Quantity);
    }


    public void ApplyCoupon(Coupon coupon)
    {
        if (coupon == null)
        {
            throw new ArgumentNullException(nameof(coupon), "Coupon cannot be null.");
        }

        // Validate and calculate the discount
        var discount = coupon.CalculateDiscount(OriginalPrice);

        if (discount > OriginalPrice)
        {
            discount = OriginalPrice;
        }

        // Set the coupon details in the order
        CouponId = coupon.Id;
        DiscountAmount = discount;
        TotalPrice = OriginalPrice - discount;
    }
}