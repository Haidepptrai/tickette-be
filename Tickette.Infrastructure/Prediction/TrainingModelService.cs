using Microsoft.ML;
using Tickette.Application.Common.Interfaces.Prediction;
using Tickette.Infrastructure.Prediction.Models;

namespace Tickette.Infrastructure.Prediction;

public class TrainingModelService : ITrainingModelService
{
    private readonly MLContext _mlContext = new();
    private readonly string _modelPath = "recommendation_model.zip";
    private readonly MockDataGenerator _mockDataGenerator;

    public TrainingModelService()
    {
        _mockDataGenerator = new MockDataGenerator();
    }

    public void TrainModel()
    {
        try
        {
            // 1. Get training data (using mock data for now)
            var trainingData = _mockDataGenerator.GenerateEventRatings();

            // 2. Load data into IDataView
            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Split data into training and test sets (80/20 split)
            var dataSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // 3. Define the training pipeline
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "UserIdEncoded",
                    inputColumnName: nameof(EventRating.UserId))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "EventIdEncoded",
                    inputColumnName: nameof(EventRating.EventId)))
                .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(
                    labelColumnName: nameof(EventRating.Label),
                    matrixColumnIndexColumnName: "UserIdEncoded",
                    matrixRowIndexColumnName: "EventIdEncoded"));

            // 4. Train the model
            Console.WriteLine("Training the model...");
            var model = pipeline.Fit(dataSplit.TrainSet);

            // 4.1 Evaluate model performance
            var predictions = model.Transform(dataSplit.TestSet);
            var metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: nameof(EventRating.Label));
            
            Console.WriteLine($"Model evaluation metrics:");
            Console.WriteLine($"RMSE: {metrics.RootMeanSquaredError}");
            Console.WriteLine($"R²: {metrics.RSquared}");
            Console.WriteLine($"MAE: {metrics.MeanAbsoluteError}");

            // 5. Save the model
            _mlContext.Model.Save(model, null, _modelPath);
            Console.WriteLine($"Model saved to {_modelPath}");

            Console.WriteLine("Model training completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error training model: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    public string GetModelPath() => _modelPath;
}

public class EventRatingPrediction
{
    public float Score { get; set; }
}