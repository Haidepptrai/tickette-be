namespace Tickette.Application.Features.Orders.Common;

public record ReservationInformation
{
    public Guid UserId { get; init; }

    public Guid TicketId { get; init; }

    public int Quantity { get; init; }

    public DateTime ReservedAt { get; init; }

    public ReservationInformation(Guid userId, Guid ticketId, int quantity)
    {
        UserId = userId;
        TicketId = ticketId;
        Quantity = quantity;
        ReservedAt = DateTime.UtcNow;
    }
}