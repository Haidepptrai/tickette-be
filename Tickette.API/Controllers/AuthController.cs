using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.DTOs.Auth;
using Tickette.Application.Features.Auth.Command;
using Tickette.Application.Features.Auth.Command.ConfirmEmail;
using Tickette.Application.Features.Auth.Command.Login;
using Tickette.Application.Features.Auth.Command.Logout;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IIdentityServices _identityServices;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(IIdentityServices identityServices, ICommandDispatcher commandDispatcher, IApplicationDbContext context, ITokenService tokenService)
        {
            _identityServices = identityServices;
            _commandDispatcher = commandDispatcher;
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ResponseDto<Guid>>> Register([FromBody] UserRegisterCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _commandDispatcher.Dispatch<UserRegisterCommand, Guid>(request, cancellationToken);

            return Ok(ResponseHandler.SuccessResponse(result, "User Created Successfully"));
        }

        [HttpPost("login")]
        public async Task<ActionResult<ResponseDto<TokenRetrieval>>> Login([FromBody] LoginCommand command,
            CancellationToken token = default)
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

        [HttpPost("logout")]
        public async Task<ActionResult<ResponseDto<bool>>> Logout([FromBody] LogoutCommand request,
            CancellationToken token = default)
        {
            var userId = GetUserId();
            request.UserId = Guid.Parse(userId);

            var result = await _commandDispatcher.Dispatch<LogoutCommand, bool>(request, token);
            return Ok(ResponseHandler.SuccessResponse(result, "Logout Successfully"));
        }

        // POST api/auth/login-google
        [HttpPost("sync-google-user")]
        public async Task<ActionResult<ResponseDto<TokenRetrieval>>> SyncGoogleUser(
            [FromBody] LoginWithGoogleCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _commandDispatcher.Dispatch<LoginWithGoogleCommand, TokenRetrieval>(request, cancellationToken);

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

        [HttpPost("multiple-register")]
        public async Task<ActionResult<ResponseDto<Guid>>> MultipleRegister(
            [FromBody] List<UserRegisterCommand> request, CancellationToken cancellationToken)
        {
            var result = new List<Guid>();
            foreach (var user in request)
            {
                var res = await _commandDispatcher.Dispatch<UserRegisterCommand, Guid>(user, cancellationToken);
                result.Add(res);
            }

            return Ok(ResponseHandler.SuccessResponse(result, "Users Created Successfully"));
        }

        [HttpPost("multiple-login")]
        public async Task<ActionResult<ResponseDto<List<string>>>> Login([FromBody] List<LoginCommand> command,
            CancellationToken cancellationToken = default)
        {
            var users = await _context.Users
                .ToListAsync(cancellationToken);

            var result = new List<string>();
            foreach (var user in users)
            {
                var token = await _tokenService.GenerateToken(user);
                result.Add(token);
            }

            return Ok(ResponseHandler.SuccessResponse(result, "Login Successfully"));
        }

        [HttpPost("multiple-account")]
        public async Task<ActionResult<ResponseDto<List<LoginCommand>>>> MultipleAccount(CancellationToken token = default)
        {
            var result = await _context.Users
                .Select(u => new LoginCommand(u.Email, "string"))
                .ToListAsync(token);

            return Ok(ResponseHandler.SuccessResponse(result, "Login Successfully"));
        }
    }
}
