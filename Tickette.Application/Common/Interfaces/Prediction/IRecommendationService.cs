namespace Tickette.Application.Common.Interfaces.Prediction;

public interface IRecommendationService
{
    /// <summary>
    /// Gets personalized event recommendations for a user
    /// </summary>
    /// <param name="userId">The user ID to get recommendations for</param>
    /// <param name="count">Number of recommendations to return</param>
    /// <returns>List of recommended event IDs</returns>
    Task<List<Guid>> GetRecommendationsAsync(Guid userId, int count = 10);
    
    /// <summary>
    /// Gets a random test user ID from the mock data
    /// </summary>
    /// <returns>A valid user GUID for testing</returns>
    Guid GetTestUserId();
}