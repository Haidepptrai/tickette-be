using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Orders.Common;
using Tickette.Application.Features.Orders.Query.ReviewOrders;
using Tickette.Application.Features.Tickets.Command;
using Tickette.Application.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;

        public OrdersController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet("my-orders")]
        [Authorize]
        public async Task<IActionResult> Get(CancellationToken cancellation)
        {
            try
            {
                // Extract UserId from JWT token claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
                if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
                {
                    return BadRequest(ResponseHandler.ErrorResponse<List<OrderedTicketGroupListDto>>(null, "User ID not found in token."));
                }

                // Set the UserId in the query object
                var query = new ReviewOrdersQuery
                {
                    UserId = Guid.Parse(userIdClaim.Value)
                };

                // Dispatch the query
                var response = await _queryDispatcher.Dispatch<ReviewOrdersQuery, ResponseDto<List<OrderedTicketGroupListDto>>>(query, cancellation);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHandler.ErrorResponse<List<OrderedTicketGroupListDto>>(null, ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderTicketsCommand command, CancellationToken cancellation)
        {
            try
            {
                var response =
                    await _commandDispatcher.Dispatch<OrderTicketsCommand, ResponseDto<Guid>>(command, cancellation);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHandler.ErrorResponse<Guid>(Guid.Empty, ex.Message));
            }
        }
    }
}
