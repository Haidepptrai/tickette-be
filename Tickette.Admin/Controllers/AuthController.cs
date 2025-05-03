using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.DTOs.Auth;
using Tickette.Application.Features.Auth.Command;
using Tickette.Application.Features.Auth.Command.Login;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;

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

        [HttpPost("login")]
        public async Task<ActionResult<ResponseDto<TokenRetrieval>>> Login([FromBody] LoginCommand command, CancellationToken token = default)
        {
            var result = await _commandDispatcher.Dispatch<LoginCommand, TokenRetrieval>(command, token);

            return Ok(ResponseHandler.SuccessResponse<object>(result, "Login Successfully"));
        }

        [HttpPost("refresh-token")]
        public async Task<ResponseDto<TokenRetrieval>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _identityServices.RefreshTokenAsync(request.RefreshToken, cancellationToken);

            return ResponseHandler.SuccessResponse(result, "Token refreshed successfully.");
        }

        [HttpDelete("{userId:guid}")]
        [Authorize(Roles = Constant.APPLICATION_ROLE.Admin)]
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
        public async Task<ActionResult<ResponseDto<TokenRetrieval>>> SyncGoogleUser([FromBody] LoginWithGoogleCommand request)
        {
            var result = await _commandDispatcher.Dispatch<LoginWithGoogleCommand, TokenRetrieval>(request, CancellationToken.None);

            return Ok(ResponseHandler.SuccessResponse(result, "Google user synced successfully."));
        }
    }
}
