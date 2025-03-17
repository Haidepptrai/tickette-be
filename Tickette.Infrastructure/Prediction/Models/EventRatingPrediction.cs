using Microsoft.ML.Data;

namespace Tickette.Infrastructure.Prediction.Models;

public class EventRatingPrediction
{
    [ColumnName("Score")]
    public float Score { get; set; }
} 