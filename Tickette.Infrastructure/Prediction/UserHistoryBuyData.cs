using Microsoft.ML.Data;

namespace Tickette.Infrastructure.Prediction;

public record UserHistoryBuyData
{
    [LoadColumn(0)]
    public string UserId { get; set; }

    [LoadColumn(1)]
    public string EventCity { get; set; }

    [LoadColumn(2)]
    public string EventDistrict { get; set; }

    [LoadColumn(3)]
    public float TicketPrice { get; set; } // Median of three ticket prices

    [LoadColumn(6, 8)]
    [VectorType(3)]
    public string[] EventHistoryCities { get; set; }  // Last 3 event locations

    [ColumnName("Label")]
    public float RecommendedScore { get; set; }

    public UserHistoryBuyData(string userId, string eventCity, string eventDistrict, float ticketPrice, string[] eventHistoryCities)
    {
        UserId = userId;
        EventCity = eventCity;
        EventDistrict = eventDistrict;
        TicketPrice = ticketPrice;
        EventHistoryCities = eventHistoryCities;
    }
}
