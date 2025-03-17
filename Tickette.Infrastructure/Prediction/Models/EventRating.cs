using Microsoft.ML.Data;

namespace Tickette.Infrastructure.Prediction.Models;

public class EventRating
{
    [LoadColumn(0)]
    public string UserId { get; set; }
    
    [LoadColumn(1)]
    public string EventId { get; set; }
    
    [LoadColumn(2)]
    public float Label { get; set; }
} 