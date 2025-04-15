using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Prediction;
using Tickette.Infrastructure.Data;
using Tickette.Infrastructure.Prediction.Models;

namespace Tickette.Infrastructure.Prediction;

public class TrainingModelService : ITrainingModelService
{
    private static readonly MLContext mlContext = new();
    private readonly TrainingDbContext _dbContext;
    private readonly IFileUploadService _fileUploadService;

    public TrainingModelService(TrainingDbContext dbContext, IFileUploadService fileUploadService)
    {
        _dbContext = dbContext;
        _fileUploadService = fileUploadService;
    }

    public async Task TrainModel()
    {
        var data = await _dbContext.UserCategoryInteractions
            .Select(uci => new UserEventInteractionViewModel()
            {
                UserId = uci.UserId.ToString(),
                EventId = uci.EventId.ToString(),
                EventType = uci.EventType,
                Location = uci.Location,
                EventDateTime = uci.EventDateTime,
                Label = uci.Label
            })
            .AsNoTracking()
            .ToListAsync();

        var dataView = mlContext.Data.LoadFromEnumerable(data);

        var options = new MatrixFactorizationTrainer.Options
        {
            MatrixColumnIndexColumnName = "UserIdEncoded",
            MatrixRowIndexColumnName = "EventIdEncoded",
            LabelColumnName = nameof(UserEventInteractionViewModel.Label),
            NumberOfIterations = 50,
            ApproximationRank = 100,
            LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass,
            Alpha = 0.01,
            Lambda = 0.025
        };

        var pipeline = mlContext.Transforms.Conversion.MapValueToKey("UserIdEncoded", nameof(UserEventInteractionViewModel.UserId))
            .Append(mlContext.Transforms.Conversion.MapValueToKey("EventIdEncoded", nameof(UserEventInteractionViewModel.EventId)))
            .Append(mlContext.Transforms.SelectColumns("UserIdEncoded", "EventIdEncoded", nameof(UserEventInteractionViewModel.Label)))
            .Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));

        var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
        var model = pipeline.Fit(split.TrainSet);

        var predictions = model.Transform(split.TestSet);
        var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: nameof(UserEventInteractionViewModel.Label));

        Console.WriteLine($"Regression Metrics:");
        Console.WriteLine($"  RMSE:      {metrics.RootMeanSquaredError:F4}");
        Console.WriteLine($"  MAE:       {metrics.MeanAbsoluteError:F4}");
        Console.WriteLine($"  R-squared: {metrics.RSquared:P2}");
        Console.WriteLine($"  Loss:      {metrics.LossFunction:F4}");

        var path = GetModelPath();
        mlContext.Model.Save(model, dataView.Schema, path);

        var modelUrl = await _fileUploadService.UploadModelAsync(path, "models");
        if (string.IsNullOrEmpty(modelUrl))
        {
            throw new ApplicationException("Failed to upload model to S3.");
        }

        //Save model URL to database
        var modelLog = new ModelLogs
        {
            ModelName = modelUrl,
            CreatedAt = DateTime.UtcNow,
        };

        await _dbContext.ModelLogs.AddAsync(modelLog);
        await _dbContext.SaveChangesAsync();

        // Delete the local model file after uploading
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            // Optionally log, but don’t block the app
            await Console.Error.WriteLineAsync($"Warning: Failed to delete local model file: {ex.Message}");
        }
    }

    public string GetModelPath()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "model.zip");
    }
}