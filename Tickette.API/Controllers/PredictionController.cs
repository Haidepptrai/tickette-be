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
        public async Task<ActionResult> Predict()
        {
            await _predictionService.TrainModel();
            return Ok("Good");
        }

        [HttpPost("recommendations")]
        public async Task<IActionResult> GetRecommendationsAsync(Guid userId)
        {
            try
            {
                var recommendations = await _recommendationService.GetRecommendationsAsync(userId);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("test-user-id")]
        public IActionResult GetTestUserId()
        {
            var testUserId = _recommendationService.GetTestUserId();
            return Ok(testUserId);
        }
    }
}
