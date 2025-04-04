using Tickette.Application.Exceptions;
using Tickette.Domain.Enums;

namespace Tickette.Domain.Entities;

public class Reservation
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; private set; }

    public ReservationStatus Status { get; private set; } = ReservationStatus.Temporary;

    private readonly List<ReservationItem> _items = new();
    public IReadOnlyCollection<ReservationItem> Items => _items.AsReadOnly();

    private Reservation() { }

    public Reservation(Guid userId, DateTime? expiresAt = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        Status = ReservationStatus.Temporary;
    }

    public void AddItem(Guid ticketId, int quantity, bool hasAssignedSeats)
    {
        if (quantity <= 0) throw new InvalidQuantityException();

        var item = new ReservationItem(Id, ticketId, quantity, hasAssignedSeats);
        _items.Add(item);
    }

    public void MarkConfirmed() => Status = ReservationStatus.Confirmed;
    public void MarkCancelled() => Status = ReservationStatus.Cancelled;
    public void MarkExpired() => Status = ReservationStatus.Expired;
}