namespace Tickette.Application.Common.Interfaces.Prediction;

public interface ITrainingModelService
{
    /// <summary>
    /// Trains the AI model.
    /// </summary>
    void TrainModel();
    
    /// <summary>
    /// Gets the path where the trained model is saved
    /// </summary>
    string GetModelPath();
}