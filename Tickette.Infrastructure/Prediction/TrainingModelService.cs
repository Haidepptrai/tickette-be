using Microsoft.ML;
using Tickette.Application.Common.Interfaces.Prediction;

namespace Tickette.Infrastructure.Prediction;

public class TrainingModelService : ITrainingModelService
{
    private readonly MLContext _mlContext = new();
    private readonly string _modelPath = "recommendation_model.zip";

    public void TrainModelAsync()
    {
        List<UserEventInteraction> userEventHistory = new();
        string[] eventCategories = { "Concert", "Sports", "Theatre", "Art", "Comedy" }; // 🔥 Different event types

        Random random = new Random();

        // Generate mock interaction history for 500 users
        for (int i = 0; i < 1000; i++)
        {
            string userId = Guid.NewGuid().ToString();
            int eventCount = random.Next(2, 5); // Each user interacts with 2 to 5 event categories

            for (int j = 0; j < eventCount; j++)
            {
                string eventCategory = eventCategories[random.Next(eventCategories.Length)];
                float clickCount = random.Next(1, 20);  // Simulate user clicking on an event multiple times
                float stayTime = random.Next(10, 300);  // Time spent on the event page (10s - 5min)
                float purchaseCount = (clickCount > 5 && stayTime > 30) ? random.Next(0, 2) : 0; // More likely to buy if high click & stay time

                userEventHistory.Add(new UserEventInteraction(userId, eventCategory, clickCount, stayTime, purchaseCount));
            }
        }

        // Load data into ML.NET
        IDataView trainingData = _mlContext.Data.LoadFromEnumerable(userEventHistory);

        // Define data preparation pipeline
        var dataProcessPipeline = _mlContext.Transforms.Conversion
            .MapValueToKey("UserId", "UserId")
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("EventCategory", "EventCategory"))
            .Append(_mlContext.Transforms.NormalizeMinMax("ClickCount", fixZero: true))
            .Append(_mlContext.Transforms.NormalizeMinMax("StayTime", fixZero: true))
            .Append(_mlContext.Transforms.NormalizeMinMax("PurchaseCount", fixZero: true));

        // Define the training pipeline
        var trainer = _mlContext.Recommendation().Trainers.MatrixFactorization(
            labelColumnName: "PurchaseCount", // Predicts how likely the user is to buy
            matrixColumnIndexColumnName: "UserId",
            matrixRowIndexColumnName: "EventCategory",
            numberOfIterations: 1000, // More training iterations
            approximationRank: 64 // Improve accuracy
        );

        var trainingPipeline = dataProcessPipeline.Append(trainer);

        // Train the model
        var trainedModel = trainingPipeline.Fit(trainingData);

        // Save the trained model
        _mlContext.Model.Save(trainedModel, trainingData.Schema, _modelPath);
        Console.WriteLine("✅ Model training completed and saved.");
    }

}

