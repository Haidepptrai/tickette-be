namespace Tickette.Application.Common.Interfaces.Prediction;

public interface IRecommendationService
{
    Task<decimal> GetRecommendationAsync(Guid userId, Guid EventId, Guid TicketId);
}