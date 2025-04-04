using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.DTOs.Auth;
using Tickette.Application.Features.Auth.Command;
using Tickette.Application.Features.Auth.Command.ConfirmEmail;
using Tickette.Application.Features.Auth.Command.Login;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tickette.API.Controllers
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
        public async Task<ActionResult<ResponseDto<Guid>>> Register([FromBody] UserRegisterCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandDispatcher.Dispatch<UserRegisterCommand, Guid>(request, cancellationToken);

            return Ok(ResponseHandler.SuccessResponse(result, "User Created Successfully"));
        }

        [HttpPost("login")]
        public async Task<ActionResult<ResponseDto<TokenRetrieval>>> Login([FromBody] LoginCommand command, CancellationToken token = default)
        {
            var result = await _commandDispatcher.Dispatch<LoginCommand, TokenRetrieval>(command, token);

            return Ok(ResponseHandler.SuccessResponse<object>(result, "Login Successfully"));
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

        // POST api/auth/login-google
        [HttpPost("sync-google-user")]
        public async Task<ActionResult<ResponseDto<TokenRetrieval>>> SyncGoogleUser([FromBody] LoginWithGoogleCommand request)
        {
            var result = await _commandDispatcher.Dispatch<LoginWithGoogleCommand, TokenRetrieval>(request, CancellationToken.None);

            return Ok(ResponseHandler.SuccessResponse(result, "Google user synced successfully."));
        }

        [HttpPost("email-confirmation")]
        public async Task<ActionResult<ResponseDto<Unit>>> ConfirmEmail(ConfirmEmailCommand query)
        {
            var result = await _commandDispatcher.Dispatch<ConfirmEmailCommand, bool>(query, CancellationToken.None);

            if (!result)
            {
                return BadRequest(ResponseHandler.ErrorResponse(Unit.Value, "Email confirmation failed."));
            }

            return Ok(ResponseHandler.SuccessResponse(result, "Email confirmed successfully."));
        }
    }
}
