using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Orders.Common;
using Tickette.Application.Features.Users.Common;
using Tickette.Application.Features.Users.Query.GetUserById;
using Tickette.Application.Wrappers;

namespace Tickette.API.Controllers
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
        // GET Current user information
        public async Task<ActionResult<ResponseDto<GetUserByIdResponse>>> GetCurrentUser(CancellationToken cancellationToken)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

                if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
                {
                    return BadRequest(
                        ResponseHandler.ErrorResponse<List<OrderedTicketGroupListDto>>(null,
                            "User ID not found in token."));
                }

                var query = new GetUserByIdQuery(Guid.Parse(userIdClaim.Value));
                var result =
                    await _queryDispatcher.Dispatch<GetUserByIdQuery, GetUserByIdResponse>(query, cancellationToken);

                return ResponseHandler.SuccessResponse(result, "Get current user successfully");
            }
            catch (Exception ex)
            {
                return ResponseHandler.ErrorResponse<GetUserByIdResponse>(null, "Internal Server Error", 500);
            }
        }
    }
}
