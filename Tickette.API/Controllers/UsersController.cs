﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Tickette.API.Helpers;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Users.Command.Client.ChangeUserImage;
using Tickette.Application.Features.Users.Command.Client.ChangeUserInformation;
using Tickette.Application.Features.Users.Common;
using Tickette.Application.Features.Users.Query.Client.GetUserById;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;

        public UsersController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost("information")]
        [SwaggerOperation(Summary = "Get current user information")]
        public async Task<ActionResult<ResponseDto<GetUserByIdResponse>>> GetCurrentUser(CancellationToken cancellationToken)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
            {
                return BadRequest(ResponseHandler.ErrorResponse(Unit.Value, "User ID not found in token."));
            }

            var query = new GetUserByIdQuery(Guid.Parse(userIdClaim.Value));
            var result = await _queryDispatcher.Dispatch<GetUserByIdQuery, GetUserByIdResponse>(query, cancellationToken);

            return ResponseHandler.SuccessResponse(result, "Get current user successfully");
        }

        [HttpPost("change-image")]
        [SwaggerOperation(Summary = "Change user image")]
        public async Task<ActionResult<ResponseDto<string>>> ChangeUserImage(IFormFile image, CancellationToken cancellationToken)
        {
            var userId = GetUserId();

            var imageTransferred = new FormFileAdapter(image);

            var query = new ChangeUserImageCommand(Guid.Parse(userId), imageTransferred);
            var result = await _commandDispatcher.Dispatch<ChangeUserImageCommand, string>(query, cancellationToken);
            return ResponseHandler.SuccessResponse(result, "Change user image successfully");
        }

        [HttpPost("change-information")]
        [SwaggerOperation(Summary = "Change user information")]
        public async Task<ActionResult<ResponseDto<Unit>>> ChangeUserInformation(ChangeUserInformationCommand command, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            command.UserId = Guid.Parse(userId);

            var result = await _commandDispatcher.Dispatch<ChangeUserInformationCommand, Unit>(command, cancellationToken);
            return ResponseHandler.SuccessResponse(result, "Change user information successfully");
        }
    }
}
