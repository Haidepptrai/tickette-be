namespace Tickette.Application.Common;

public class RecommendationResult
{
    public string EventCategory { get; set; } // 🔥 Now uses category instead of city
    public float PredictedTicketPrice { get; set; } // 🔥 Predicted price if available
    public float Score { get; set; } // 🔥 Relevance score (higher = better recommendation)

    public RecommendationResult(string eventCategory, float predictedTicketPrice, float score)
    {
        EventCategory = eventCategory;
        PredictedTicketPrice = predictedTicketPrice;
        Score = score;
    }
}
