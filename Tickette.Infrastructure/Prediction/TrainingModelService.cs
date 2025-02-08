using Microsoft.ML;
using Microsoft.ML.Transforms;
using Tickette.Application.Common.Interfaces.Prediction;

namespace Tickette.Infrastructure.Prediction;

public class TrainingModelService : ITrainingModelService
{
    private readonly MLContext _mlContext = new();
    private ITransformer _trainedModel;
    private readonly string _modelPath = "recommendation_model.zip";

    public void TrainModelAsync()
    {
        List<UserHistoryBuyData> userHistoryData =
        [
            new UserHistoryBuyData(
                Guid.NewGuid().ToString(), "Ho Chi Minh", "District 1",
                500000f, new string[] { "Ho Chi Minh", "Ho Chi Minh", "Ha Noi" }),


            new UserHistoryBuyData(
                Guid.NewGuid().ToString(), "Ho Chi Minh", "District 1",
                450000f, new string[] { "Ho Chi Minh", "Ho Chi Minh", "Ha Noi" }),


            new UserHistoryBuyData(
                Guid.NewGuid().ToString(), "Ho Chi Minh", "District 1",
                550000f, new string[] { "Ho Chi Minh", "Ho Chi Minh", "Ha Noi" }),


            new UserHistoryBuyData(
                Guid.NewGuid().ToString(), "Ha Noi", "Ba Dinh",
                700000f, new string[] { "Ha Noi", "Ha Noi", "Ha Noi" }),


            new UserHistoryBuyData(
                Guid.NewGuid().ToString(), "Ha Noi", "Ba Dinh",
                750000f, new string[] { "Ha Noi", "Ha Noi", "Ha Noi" }),


            new UserHistoryBuyData(
                Guid.NewGuid().ToString(), "Ha Noi", "Ba Dinh",
                800000f, new string[] { "Ha Noi", "Ha Noi", "Ha Noi" })
        ];

        // Load data
        IDataView trainingData = _mlContext.Data.LoadFromEnumerable(userHistoryData);

        // Define data preparation pipeline
        var dataProcessPipeline = _mlContext.Transforms.Conversion
            // Convert UserId and EventCity to Key type (Needed for Matrix Factorization)
            .MapValueToKey("UserIdKey", "UserId")
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("EventCityKey", "EventCity"))

            // Convert categorical text columns to numerical format
            .Append(_mlContext.Transforms.Categorical.OneHotEncoding("EventDistrictEncoded", "EventDistrict"))

            // Transform event history as separate categorical features
            .Append(_mlContext.Transforms.Categorical.OneHotEncoding("EventHistory1Encoded", "EventHistoryCities", outputKind: OneHotEncodingEstimator.OutputKind.Indicator))
            .Append(_mlContext.Transforms.Categorical.OneHotEncoding("EventHistory2Encoded", "EventHistoryCities", outputKind: OneHotEncodingEstimator.OutputKind.Indicator))
            .Append(_mlContext.Transforms.Categorical.OneHotEncoding("EventHistory3Encoded", "EventHistoryCities", outputKind: OneHotEncodingEstimator.OutputKind.Indicator))

            // Concatenate all features (Do not include Key columns in concatenation)
            .Append(_mlContext.Transforms.Concatenate("Features",
                "EventDistrictEncoded",
                "EventHistory1Encoded",
                "EventHistory2Encoded",
                "EventHistory3Encoded",
                "TicketPrice"))

            // Normalize data to improve training stability
            .Append(_mlContext.Transforms.NormalizeMinMax("Features"));

        // Define the training pipeline
        var trainer = _mlContext.Recommendation().Trainers.MatrixFactorization(
            labelColumnName: "TicketPrice",  // ✅ Ensure this matches the label column
            matrixColumnIndexColumnName: "UserIdKey",  // ✅ Use the Key column
            matrixRowIndexColumnName: "EventCityKey"   // ✅ Use the Key column
        );

        // Combine data process pipeline with the trainer
        var trainingPipeline = dataProcessPipeline.Append(trainer);

        // Train the model
        _trainedModel = trainingPipeline.Fit(trainingData);

        // Save the trained model
        _mlContext.Model.Save(_trainedModel, trainingData.Schema, _modelPath);
        Console.WriteLine("✅ Model training completed and saved.");
    }
}

