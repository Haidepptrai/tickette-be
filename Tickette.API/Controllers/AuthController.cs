using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Helpers;

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
            var (result, userId) = await _identityServices.CreateUserAsync(request.UserEmail, request.Password);

            if (result.Succeeded)
            {
                return Ok(new { UserId = userId });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<ResponseDto<object>> Login([FromBody] LoginModel model)
        {
            var (result, token, refreshToken) = await _identityServices.LoginAsync(model.UserEmail, model.Password);

            if (!result.Succeeded)
            {
                return ResponseHandler.ErrorResponse<object>(result);
            }

            var data = new
            {
                Token = token,
                RefreshToken = refreshToken
            };

            return ResponseHandler.SuccessResponse<object>(data, "Login Successfully");

        }

        [HttpPost("refresh-token")]
        public async Task<ResponseDto<object>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var (result, token, refreshToken) = await _identityServices.RefreshTokenAsync(request.Token, request.RefreshToken);

            if (!result.Succeeded)
            {
                return ResponseHandler.ErrorResponse<object>(result);
            }

            var data = new
            {
                Token = token,
                RefreshToken = refreshToken
            };

            return ResponseHandler.SuccessResponse<object>(data, "Token refreshed successfully.");
        }

        public class RefreshTokenRequest
        {
            public string Token { get; set; }
            public string RefreshToken { get; set; }
        }

        public class RegisterRequest
        {
            [EmailAddress]
            public string UserEmail { get; set; }

            [PasswordPropertyText]
            public string Password { get; set; }
        }

        public class LoginModel
        {
            [EmailAddress]
            public string UserEmail { get; set; }

            [PasswordPropertyText]
            public string Password { get; set; }
        }

        [HttpGet("{userId:guid}/role/{role}")]
        public async Task<IActionResult> IsInRole(Guid userId, string role)
        {
            var isInRole = await _identityServices.IsInRoleAsync(userId, role);
            return Ok(new { IsInRole = isInRole });
        }

        [HttpGet("{userId:guid}/authorize/{policyName}")]
        public async Task<IActionResult> Authorize(Guid userId, string policyName)
        {
            var isAuthorized = await _identityServices.AuthorizeAsync(userId, policyName);
            return Ok(new { IsAuthorized = isAuthorized });
        }


        [HttpDelete("{userId:guid}")]
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
