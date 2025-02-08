using Tickette.Application.Common.Interfaces.Prediction;
using Tickette.Application.Common.MachineLearning;

namespace Tickette.Infrastructure.Prediction;

public class RecommendationService : IRecommendationService
{
    private readonly ITrainingModelService _predictionService;

    public RecommendationService(ITrainingModelService predictionService)
    {
        _predictionService = predictionService;
    }

    public Task<decimal> GetRecommendationAsync(Guid userId, Guid eventId, Guid ticketId)
    {
        var input = new ModelInput
        {
            UserId = userId,
            EventId = eventId,
            TicketId = ticketId
        };

        //var prediction = _predictionService.Predict(input);
        //return Task.FromResult(prediction.RecommendedTicketPrice);
        return Task.FromResult<decimal>(22);
    }
}
