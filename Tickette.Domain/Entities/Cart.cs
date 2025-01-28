using Tickette.Domain.Common;
using Tickette.Domain.Enums;
using Tickette.Domain.ValueObjects;

namespace Tickette.Domain.Entities;

public sealed class Cart : BaseEntity
{
    public Guid UserId { get; private set; }

    public DateTime ExpiryTime { get; private set; }

    public CartStatus Status { get; private set; } = CartStatus.Active;

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> CartItems => _items.AsReadOnly();

    public User User { get; set; }

    public Cart(Guid userId)
    {
        UserId = userId;
        ExpiryTime = DateTime.UtcNow.AddMinutes(15);
    }

    public void AddItem(Guid ticketId, int quantity, Price price)
    {
        var existingItem = CartItems.FirstOrDefault(i => i.Id == ticketId);

        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            _items.Add(CartItem.Create(ticketId, quantity, price));
        }
    }

    // Remove an item from the cart
    public void RemoveItem(Guid ticketId)
    {
        var item = CartItems.FirstOrDefault(i => i.TicketId == ticketId);
        if (item != null)
        {
            _items.Remove(item);
        }
    }

    // Calculate the total cart price
    public decimal GetTotalPrice() => _items.Sum(i => i.Price.Amount);

    // Check if the cart is expired
    public bool IsExpired() => DateTime.UtcNow > ExpiryTime;

    // Mark the cart as completed
    public void Complete()
    {
        if (IsExpired())
            throw new InvalidOperationException("Cannot complete an expired cart.");

        Status = CartStatus.Completed;
    }
}
