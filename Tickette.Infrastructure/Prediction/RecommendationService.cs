using Microsoft.ML;
using Tickette.Application.Common.Interfaces.Prediction;
using Tickette.Infrastructure.Prediction.Models;

namespace Tickette.Infrastructure.Prediction;

public class RecommendationService : IRecommendationService
{
    private readonly ITrainingModelService _trainingModelService;
    private readonly MockDataGenerator _mockDataGenerator;
    private readonly MLContext _mlContext = new();
    private PredictionEngine<EventRating, EventRatingPrediction> _predictionEngine;

    public RecommendationService(ITrainingModelService trainingModelService)
    {
        _trainingModelService = trainingModelService;
        _mockDataGenerator = new MockDataGenerator();
    }

    private void EnsureModelLoaded()
    {
        if (_predictionEngine == null)
        {
            var modelPath = _trainingModelService.GetModelPath();
            if (!File.Exists(modelPath))
            {
                throw new InvalidOperationException("Model not trained. Please call TrainModel() first.");
            }
            var mlModel = _mlContext.Model.Load(modelPath, out _);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<EventRating, EventRatingPrediction>(mlModel);
        }
    }

    public async Task<List<Guid>> GetRecommendationsAsync(Guid userId, int count = 10)
    {
        try
        {
            EnsureModelLoaded();

            // Get candidate events for this user
            var candidateEvents = _mockDataGenerator.GetCandidateEventsForUser(userId);

            // Score each candidate event
            var scores = new List<(Guid EventId, float Score)>();
            foreach (var eventId in candidateEvents)
            {
                var prediction = _predictionEngine.Predict(new EventRating
                {
                    UserId = userId.ToString(),
                    EventId = eventId.ToString(),
                    Label = 0 // Doesn't matter for prediction
                });

                scores.Add((eventId, prediction.Score));
            }

            // Return top N recommendations
            return scores.OrderByDescending(x => x.Score)
                        .Take(count)
                        .Select(x => x.EventId)
                        .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting recommendations: {ex.Message}");
            // Return empty list instead of throwing to prevent application crashes
            return new List<Guid>();
        }
    }

    /// <summary>
    /// Gets a random test user ID from the mock data
    /// </summary>
    /// <returns>A valid user GUID for testing</returns>
    public Guid GetTestUserId()
    {
        var userIds = _mockDataGenerator.GetAllUserIds();
        if (userIds.Count == 0)
        {
            // Force generation of mock data if not already done
            _mockDataGenerator.GenerateEventRatings();
            userIds = _mockDataGenerator.GetAllUserIds();
        }

        return userIds.FirstOrDefault();
    }
}