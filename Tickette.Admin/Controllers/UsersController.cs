﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Users.Common;
using Tickette.Application.Features.Users.Query.Admin.AdminGetUserById;
using Tickette.Application.Features.Users.Query.Admin.GetAllUsers;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;

namespace Tickette.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly IIdentityServices _identityServices;

        public UsersController(IQueryDispatcher queryDispatcher, IIdentityServices identityServices)
        {
            _queryDispatcher = queryDispatcher;
            _identityServices = identityServices;
        }

        [HttpPost("all")]
        [Authorize(Roles = $"{Constant.APPLICATION_ROLE.Admin},{Constant.APPLICATION_ROLE.Moderator}")]
        public async Task<ActionResult<ResponseDto<PreviewUserResponse>>> GetAllUsers([FromBody] GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            if (request.PageNumber < 1 || request.PageSize < 1)
            {
                return BadRequest(ResponseHandler.ErrorResponse(Unit.Value, "Invalid Request"));
            }

            var result = await _queryDispatcher.Dispatch<GetAllUsersQuery, PagedResult<PreviewUserResponse>>(request, cancellationToken);

            var paginationMeta = new PaginationMeta(request.PageNumber, request.PageSize, result.TotalCount, result.TotalPages);

            return Ok(ResponseHandler.PaginatedResponse(result.Items, paginationMeta, "Users retrieved successfully"));
        }

        // Get user by ID, Update user, Delete user, etc.
        [HttpPost("id")]
        [Authorize(Roles = $"{Constant.APPLICATION_ROLE.Admin},{Constant.APPLICATION_ROLE.Moderator}")]
        public async Task<ActionResult<ResponseDto<GetUserByIdResponse>>> GetUserById([FromBody] GetUserByIdRequest body, CancellationToken cancellationToken)
        {
            var request = new AdminGetUserByIdQuery(body.UserId);
            var result = await _queryDispatcher.Dispatch<AdminGetUserByIdQuery, GetUserByIdResponse>(request, cancellationToken);

            return Ok(ResponseHandler.SuccessResponse(result, "User retrieved successfully"));
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = $"{Constant.APPLICATION_ROLE.Admin}")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            var result = await _identityServices.AssignToRoleAsync(request.UserId, request.RoleId);

            if (result)
            {
                return Ok(ResponseHandler.SuccessResponse(Unit.Value, "Role assigned successfully"));
            }

            return BadRequest(ResponseHandler.ErrorResponse(Unit.Value, "An error has occurred"));
        }

        public class AssignRoleRequest
        {
            public Guid UserId { get; set; }
            public IEnumerable<Guid> RoleId { get; set; }
        }
    }
}
