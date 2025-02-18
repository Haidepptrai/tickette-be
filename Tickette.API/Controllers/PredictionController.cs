using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.Interfaces.Prediction;

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        private readonly ITrainingModelService _predictionService;
        private readonly IRecommendationService _recommendationService;

        public PredictionController(ITrainingModelService predictionService, IRecommendationService recommendationService)
        {
            _predictionService = predictionService;
            _recommendationService = recommendationService;
        }

        [HttpPost("ai-training")]
        public IActionResult Predict()
        {
            try
            {
                _predictionService.TrainModel();
                return Ok("Good");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("recommendations")]
        public async Task<IActionResult> GetRecommendationsAsync(Guid userId)
        {
            try
            {
                //var recommendations = await _recommendationService.GetRecommendationsAsync(userId);
                //return Ok(recommendations);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
