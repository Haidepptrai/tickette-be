using Microsoft.ML.Data;

namespace Tickette.Infrastructure.Prediction;

public record UserEventInteraction
{
    [LoadColumn(0)]
    public string UserId { get; set; }

    [LoadColumn(1)]
    public string EventCategory { get; set; }

    [LoadColumn(2)]
    public float ClickCount { get; set; }  // How many times the user clicked this event type

    [LoadColumn(3)]
    public float StayTime { get; set; }  // How long they stayed on the event page (seconds)

    [LoadColumn(4)]
    public float PurchaseCount { get; set; }  // How many times they actually bought this event type

    public UserEventInteraction(string userId, string eventCategory, float clickCount, float stayTime, float purchaseCount)
    {
        UserId = userId;
        EventCategory = eventCategory;
        ClickCount = clickCount;
        StayTime = stayTime;
        PurchaseCount = purchaseCount;
    }
}

