using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tickette.Application.Common.Interfaces.Prediction;

namespace Tickette.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestRecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly ITrainingModelService _trainingModelService;

    public TestRecommendationController(
        IRecommendationService recommendationService,
        ITrainingModelService trainingModelService)
    {
        _recommendationService = recommendationService;
        _trainingModelService = trainingModelService;
    }

    [HttpGet("train")]
    public ActionResult TrainModel()
    {
        try
        {
            _trainingModelService.TrainModel();
            return Ok("Model trained successfully");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error training model: {ex.Message}");
        }
    }

    [HttpGet("test-user")]
    public ActionResult<Guid> GetTestUser()
    {
        var userId = _recommendationService.GetTestUserId();
        return Ok(userId);
    }

    [HttpGet("recommendations")]
    public async Task<ActionResult<List<Guid>>> GetRecommendations([FromQuery] Guid? userId = null, [FromQuery] int count = 5)
    {
        try
        {
            // Use provided userId or get a test one
            var userIdToUse = userId ?? _recommendationService.GetTestUserId();
            
            var recommendations = await _recommendationService.GetRecommendationsAsync(userIdToUse, count);
            return Ok(new 
            {
                UserId = userIdToUse,
                Recommendations = recommendations
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error getting recommendations: {ex.Message}");
        }
    }
} 