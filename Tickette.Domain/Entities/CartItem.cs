using Tickette.Domain.Common;
using Tickette.Domain.ValueObjects;

namespace Tickette.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }

    public Guid TicketId { get; private set; }

    public Price Price { get; private set; }

    public int Quantity { get; private set; }

    public Ticket Ticket { get; private set; }

    public Cart Cart { get; private set; }

    private CartItem() { }


    private CartItem(Guid ticketId, int quantity, Price price)
    {
        TicketId = ticketId;
        Quantity = quantity;
        Price = price;
    }

    public static CartItem Create(Guid ticketId, int quantity, Price price)
    {
        if (ticketId == Guid.Empty)
            throw new ArgumentException("TicketId cannot be empty.");
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");
        if (price == null)
            throw new ArgumentNullException(nameof(price));

        return new CartItem(ticketId, quantity, price);
    }


    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");
        Quantity = newQuantity;
    }
}
