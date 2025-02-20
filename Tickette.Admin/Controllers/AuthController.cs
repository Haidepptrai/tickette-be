using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Models;
using Tickette.Application.DTOs.Auth;
using Tickette.Application.Features.Auth.Command.Login;
using Tickette.Application.Wrappers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tickette.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityServices _identityServices;
        private readonly ICommandDispatcher _commandDispatcher;

        public AuthController(IIdentityServices identityServices, ICommandDispatcher commandDispatcher)
        {
            _identityServices = identityServices;
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _identityServices.CreateUserAsync(request.Email, request.Password);

            if (result.Succeeded)
            {
                return Ok(ResponseHandler.SuccessResponse<object>(null, "User Created Successfully"));
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<ResponseDto<object>> Login([FromBody] LoginCommand command, CancellationToken token = default)
        {
            var result = await _commandDispatcher.Dispatch<LoginCommand, AuthResult<TokenRetrieval>>(command, token);

            if (!result.Succeeded)
            {
                return ResponseHandler.ErrorResponse<object>(result);
            }

            return ResponseHandler.SuccessResponse<object>(result.Data, "Login Successfully");
        }

        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<ResponseDto<object>> RefreshToken([FromBody] string refreshToken)
        {
            var result = await _identityServices.RefreshTokenAsync(refreshToken);

            if (!result.Succeeded)
            {
                return ResponseHandler.ErrorResponse<object>(result);
            }

            return ResponseHandler.SuccessResponse<object>(result.Data, "Token refreshed successfully.");
        }

        public class RegisterRequest
        {
            [EmailAddress]
            public string Email { get; set; }

            [PasswordPropertyText]
            public string Password { get; set; }
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

        // POST api/auth/login-google
        [HttpPost("sync-google-user")]
        public async Task<IActionResult> SyncGoogleUser([FromBody] GoogleUserRequest request)
        {
            var result = await _identityServices.SyncGoogleUserAsync(request);

            if (!result.Succeeded)
            {
                return BadRequest(ResponseHandler.ErrorResponse("Google user sync failed."));
            }

            return Ok(ResponseHandler.SuccessResponse(result.Data, "Google user synced successfully."));
        }
    }
}
