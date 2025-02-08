using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.Interfaces.Prediction;

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        private readonly ITrainingModelService _predictionService;

        public PredictionController(ITrainingModelService predictionService)
        {
            _predictionService = predictionService;
        }

        [HttpPost("ai-training")]
        public IActionResult Predict()
        {
            try
            {
                _predictionService.TrainModelAsync();
                return Ok("Good");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
