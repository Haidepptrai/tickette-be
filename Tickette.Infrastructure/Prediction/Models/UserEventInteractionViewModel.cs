namespace Tickette.Infrastructure.Prediction.Models;

public class UserEventInteractionViewModel
{
    public string UserId { get; set; }
    public string EventId { get; set; }   // Primary recommendation key
    public string EventType { get; set; }
    public string Location { get; set; }
    public DateTime EventDateTime { get; set; }
    public float Label { get; set; } // 1.0 = strong interest, 0.5 = click, 0.0 = no action
}