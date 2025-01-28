namespace Tickette.Application.Events.Models;

public record CartExpiredEvent
{
    public Guid CartId { get; set; }
    public DateTime ExpiryTime { get; set; }
}