using Tickette.Application.Common.Interfaces.Prediction;

namespace Tickette.Infrastructure.Prediction;

public class RecommendationService : IRecommendationService
{
    public Task<List<Guid>> GetRecommendationsAsync(Guid userId, int count = 10)
    {
        throw new NotImplementedException();
    }

    public Guid GetTestUserId()
    {
        throw new NotImplementedException();
    }
}