namespace Tickette.Application.Common;

public class ReservationData
{
    public Guid UserId { get; set; }
    public int Quantity { get; set; }
    public DateTime ReservedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}