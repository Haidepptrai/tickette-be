using Microsoft.ML;
using Tickette.Application.Common;
using Tickette.Application.Common.Interfaces.Prediction;

namespace Tickette.Infrastructure.Prediction;

public class RecommendationService : IRecommendationService
{
    private readonly ITrainingModelService _trainingModelService;
    private readonly MLContext _mlContext = new();
    private const string ModelPath = "recommendation_model.zip";
    private readonly ITransformer? _trainedModel;

    public RecommendationService(ITrainingModelService trainingModelService)
    {
        _trainingModelService = trainingModelService;

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

    public Task<List<RecommendationResult>> GetRecommendationsAsync(Guid userId)
    {
        if (_trainedModel == null)
        {
            return Task.FromResult(new List<RecommendationResult>());
        }

        // 🔹 Generate Mock Data for Testing
        List<UserEventInteraction> userInteractions = GenerateMockUserEventPairs(userId);

        // 🔹 Convert test data to IDataView
        IDataView testDataView = _mlContext.Data.LoadFromEnumerable(userInteractions);

        // 🔹 Make predictions
        IDataView predictions = _trainedModel.Transform(testDataView);
        if (predictions == null)
        {
            throw new Exception("⚠️ Model transformation failed. Predictions returned null.");
        }

        // 🔹 Convert predictions to a list
        var predictionList = _mlContext.Data.CreateEnumerable<RecommendationPrediction>(predictions, reuseRowObject: false).ToList();

        // 🔹 Sort recommendations based on highest scores
        var recommendationResults = predictionList
            .OrderByDescending(p => p.Score)
            .Take(5) // ✅ Return only top 5 best recommendations
            .Select(p => new RecommendationResult(
                GetEventCategoryName(p.EventCategory),
                PredictTicketPrice(p.EventCategory), // 🔥 Predict price dynamically
                p.Score
            ))
            .ToList();

        return Task.FromResult(recommendationResults);
    }

    /// 🔹 Generate mock interactions (NO DATABASE CALLS)
    private List<UserEventInteraction> GenerateMockUserEventPairs(Guid userId)
    {
        string[] eventCategories = { "Concert", "Sports", "Theatre", "Art", "Comedy" };
        List<UserEventInteraction> mockTestData = new();
        Random random = new Random();

        for (int i = 0; i < 10; i++) // 🔥 Generate 10 interactions per user (increase training data)
        {
            string eventCategory = eventCategories[random.Next(eventCategories.Length)];
            float clickCount = random.Next(5, 50); // 🔥 Ensure users click a lot
            float stayTime = random.Next(20, 600); // 🔥 Ensure users stay long enough

            // 🔥 Increase purchase probability if they interacted heavily
            float purchaseCount = (clickCount > 10 && stayTime > 30) ? random.Next(1, 3) : 0;

            mockTestData.Add(new UserEventInteraction(userId.ToString(), eventCategory, clickCount, stayTime, purchaseCount));
        }

        return mockTestData;
    }


    /// 🔹 Helper function to map event category ID to a readable name
    private string GetEventCategoryName(uint eventCategoryId)
    {
        string[] eventCategories = { "Concert", "Sports", "Theatre", "Art", "Comedy" };
        return eventCategoryId < eventCategories.Length ? eventCategories[eventCategoryId] : "Unknown";
    }

    /// 🔹 Predict ticket price dynamically (mocked values)
    private float PredictTicketPrice(uint eventCategoryId)
    {
        float[] avgPrices = { 250000, 300000, 400000, 150000, 180000 }; // Example avg prices per category
        return eventCategoryId < avgPrices.Length ? avgPrices[eventCategoryId] : 200000;
    }

    /// 🔹 Prediction result model
    public class RecommendationPrediction
    {
        public float Score { get; set; }
        public uint EventCategory { get; set; }
    }
}
