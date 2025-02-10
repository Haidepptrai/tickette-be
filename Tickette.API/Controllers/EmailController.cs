using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.Interfaces.Email;

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return BadRequest("Invalid unsubscribe request.");

            if (!_emailService.ValidateUnsubscribeToken(email, token))
                return Unauthorized("Invalid token.");

            bool success = await _emailService.UnsubscribeEmailAsync(email);
            if (!success)
                return BadRequest("You are already unsubscribed.");

            return Ok("You have been unsubscribed successfully.");
        }
    }
}
