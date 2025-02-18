using Microsoft.ML;
using Tickette.Application.Common.Interfaces.Prediction;

namespace Tickette.Infrastructure.Prediction;

public class RecommendationService : IRecommendationService
{
    private readonly MLContext _mlContext = new();
    private const string ModelPath = "recommendation_model.zip";
    private readonly ITransformer? _trainedModel;

    public RecommendationService()
    {
        // Load the trained model
        if (File.Exists(ModelPath))
        {
            _trainedModel = _mlContext.Model.Load(ModelPath, out _);
        }
        else
        {
            Console.WriteLine("⚠️ Model file not found. Please train the model first.");
        }
    }


    public void GetRecommendationsAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}