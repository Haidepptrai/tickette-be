using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Orders.Command.CreateOrder;
using Tickette.Application.Features.Orders.Common;
using Tickette.Application.Features.Orders.Query.ReviewOrders;
using Tickette.Application.Features.QRCode.Common;
using Tickette.Application.Features.QRCode.Queries;
using Tickette.Application.Features.QRCode.Queries.ValidateQrCode;
using Tickette.Application.Features.Tickets.Command;
using Tickette.Application.Wrappers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tickette.API.Controllers;

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
                return BadRequest(
                    ResponseHandler.ErrorResponse<List<OrderedTicketGroupListDto>>(null,
                        "User ID not found in token."));
            }

            // Set the UserId in the query object
            var query = new ReviewOrdersQuery
            {
                UserId = Guid.Parse(userIdClaim.Value)
            };

            // Dispatch the query
            var response =
                await _queryDispatcher.Dispatch<ReviewOrdersQuery, ResponseDto<List<OrderedTicketGroupListDto>>>(
                    query, cancellation);

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

    [HttpGet("detail/get-qrcode/{orderItemId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetQrCode(Guid orderItemId, CancellationToken cancellation)
    {
        try
        {
            // Extract UserId from JWT token claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
            {
                return BadRequest(ResponseHandler.ErrorResponse<byte[]>(null, "User ID not found in token."));
            }

            // Set the UserId and OrderItemId in the query object
            var query = new GetQrCodeQuery
            {
                UserId = Guid.Parse(userIdClaim.Value),
                OrderItemId = orderItemId
            };

            // Dispatch the query
            var response =
                await _queryDispatcher.Dispatch<GetQrCodeQuery, ResponseDto<byte[]>>(query, cancellation);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ResponseHandler.ErrorResponse<byte[]>(null, ex.Message));
        }
    }

    [HttpPost("validate-qrcode")]
    [Authorize]
    public async Task<IActionResult> ValidateQrCode([FromBody] ValidateQrCodeQuery query,
        CancellationToken cancellation)
    {
        try
        {
            var response =
                await _queryDispatcher.Dispatch<ValidateQrCodeQuery, ResponseDto<DataRetrievedFromQrCode>>(query,
                    cancellation);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ResponseHandler.ErrorResponse(false, ex.Message));
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

    [HttpPost("create")]
    [Authorize]
    public async Task<ResponseDto<string>> CreateOrder([FromBody] CreateOrderCommand command,
        CancellationToken cancellation)
    {
        try
        {
            var response =
                await _commandDispatcher.Dispatch<CreateOrderCommand, string>(command, cancellation);

            return ResponseHandler.SuccessResponse(response, "Retrieving Stripe Secret Key");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ErrorResponse("We fucked up!", ex.Message);
        }
    }
}