namespace Tickette.Infrastructure.Prediction.Models;

public class PreprocessedEventModel
{
    public string Id { get; set; }
    public string ProcessedText { get; set; }
    public float PriceMin { get; set; }
    public float PriceMax { get; set; }
}