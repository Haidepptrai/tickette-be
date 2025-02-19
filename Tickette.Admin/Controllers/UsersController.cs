using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Models;
using Tickette.Application.Features.Users.Query.GetAllUsers;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;
using Tickette.Domain.Entities;

namespace Tickette.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;

        public UsersController(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [HttpPost]
        public async Task<ActionResult> GetAllUsers([FromBody] GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            if (request.PageNumber < 1 || request.PageSize < 1)
            {
                return BadRequest(ResponseHandler.ErrorResponse(Unit.Value, "Invalid Request"));
            }

            var result = await _queryDispatcher.Dispatch<GetAllUsersQuery, AuthResult<IEnumerable<User>>>(request, cancellationToken);

            if (!result.Succeeded)
            {
                return NotFound(ResponseHandler.ErrorResponse(Unit.Value, "Users not found"));
            }

            var totalItems = result.Data!.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);

            var paginationMeta = new PaginationMeta(request.PageNumber, request.PageSize, totalItems, totalPages);

            return Ok(ResponseHandler.PaginatedResponse(result.Data, paginationMeta, "Users retrieved successfully"));
        }

    }
}
