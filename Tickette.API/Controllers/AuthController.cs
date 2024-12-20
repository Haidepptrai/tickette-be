using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityServices _identityServices;

        public AuthController(IIdentityServices identityServices)
        {
            _identityServices = identityServices;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var (result, userId) = await _identityServices.CreateUserAsync(request.UserName, request.Password);

            if (result.Succeeded)
            {
                return Ok(new { UserId = userId });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var (result, token) = await _identityServices.LoginAsync(model.UserName, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { Token = token });
        }

        public class RegisterRequest
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public class LoginModel
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        [HttpGet("{userId}/email")]
        public async Task<IActionResult> GetUserEmail(Guid userId)
        {
            var email = await _identityServices.GetUserEmailAsync(userId);

            if (email == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new { Email = email });
        }


        [HttpGet("{userId}/role/{role}")]
        public async Task<IActionResult> IsInRole(Guid userId, string role)
        {
            var isInRole = await _identityServices.IsInRoleAsync(userId, role);
            return Ok(new { IsInRole = isInRole });
        }

        [HttpGet("{userId}/authorize/{policyName}")]
        public async Task<IActionResult> Authorize(Guid userId, string policyName)
        {
            var isAuthorized = await _identityServices.AuthorizeAsync(userId, policyName);
            return Ok(new { IsAuthorized = isAuthorized });
        }


        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var result = await _identityServices.DeleteUserAsync(userId);

            if (result.Succeeded)
            {
                return Ok("User deleted successfully.");
            }

            return BadRequest(result.Errors);
        }

    }
}
