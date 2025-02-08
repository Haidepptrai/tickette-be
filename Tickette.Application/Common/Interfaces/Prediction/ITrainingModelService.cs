namespace Tickette.Application.Common.Interfaces.Prediction;

public interface ITrainingModelService
{
    /// <summary>
    /// Trains the AI model.
    /// </summary>
    void TrainModelAsync();
}