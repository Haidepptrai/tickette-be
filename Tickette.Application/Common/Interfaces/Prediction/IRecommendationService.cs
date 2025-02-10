namespace Tickette.Application.Common.Interfaces.Prediction;

public interface IRecommendationService
{
    public Task<List<RecommendationResult>> GetRecommendationsAsync(Guid userId);
}