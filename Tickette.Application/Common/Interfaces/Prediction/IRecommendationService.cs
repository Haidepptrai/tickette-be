namespace Tickette.Application.Common.Interfaces.Prediction;

public interface IRecommendationService
{
    public void GetRecommendationsAsync(Guid userId);
}