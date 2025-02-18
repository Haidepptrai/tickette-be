using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Orders.Command.CreateOrder;
using Tickette.Application.Features.Orders.Command.RemoveReserveTicket;
using Tickette.Application.Features.Orders.Command.ReserveTicket;
using Tickette.Application.Features.Orders.Common;
using Tickette.Application.Features.Orders.Query.ReviewOrders;
using Tickette.Application.Features.Orders.Query.ValidateReservation;
using Tickette.Application.Features.QRCode.Common;
using Tickette.Application.Features.QRCode.Queries.ValidateQrCode;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;

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

    [HttpPost("validate-qrcode")]
    [Authorize(Policy = "CheckInAccess")]
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

    [HttpPost("create")]
    [SwaggerOperation(summary: "Create order for the user after confirm payment")]
    [Authorize]
    public async Task<ResponseDto<CreateOrderResponse>> CreateOrder([FromBody] CreateOrderCommand command,
        CancellationToken cancellation)
    {
        try
        {
            var response =
                await _commandDispatcher.Dispatch<CreateOrderCommand, CreateOrderResponse>(command, cancellation);

            return ResponseHandler.SuccessResponse(response, "Order created successfully");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ErrorResponse<CreateOrderResponse>(null, "An error while creating order");
        }
    }

    [HttpPost("validate-reservation")]
    [SwaggerOperation(summary: "Validate the reservation before creating the order")]
    [Authorize]
    public async Task<ActionResult<ResponseDto<bool>>> ValidateReservation([FromBody] ValidateReservationQuery query,
        CancellationToken cancellation)
    {
        try
        {
            var response =
                await _queryDispatcher.Dispatch<ValidateReservationQuery, bool>(query, cancellation);

            return Ok(ResponseHandler.SuccessResponse(response, "Tickets is still in reserved"));
        }
        catch (Exception ex)
        {
            return BadRequest(ResponseHandler.ErrorResponse(false, ex.Message));
        }
    }

    [HttpPost("reserve-tickets")]
    [SwaggerOperation(summary: "Reserve tickets for the user before confirm payment")]
    [Authorize]
    public async Task<ActionResult<ResponseDto<Unit>>> ReserveTickets([FromBody] ReserveTicketCommand command,
        CancellationToken cancellation)
    {
        try
        {
            var response =
                await _commandDispatcher.Dispatch<ReserveTicketCommand, Unit>(command, cancellation);
            return Ok(ResponseHandler.SuccessResponse(response, "Tickets reserved successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ResponseHandler.ErrorResponse(Unit.Value, ex.Message));
        }
    }

    [HttpPost("remove-tickets-reserve")]
    [SwaggerOperation(summary: "Remove the reservation of the tickets")]
    [Authorize]
    public async Task<ActionResult<ResponseDto<Unit>>>
        RemoveReserve([FromBody] RemoveReserveTicketCommand command, CancellationToken cancellation)
    {
        try
        {
            var response =
                await _commandDispatcher.Dispatch<RemoveReserveTicketCommand, Unit>(command, cancellation);
            return Ok(ResponseHandler.SuccessResponse(response, "Reservation removed successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ResponseHandler.ErrorResponse(Unit.Value, ex.Message));
        }
    }

}