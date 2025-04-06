using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Orders.Command.CreateOrder;
using Tickette.Application.Features.Orders.Command.RemoveReserveTicket;
using Tickette.Application.Features.Orders.Command.ReserveTicket;
using Tickette.Application.Features.Orders.Common;
using Tickette.Application.Features.Orders.Query.ReviewOrders;
using Tickette.Application.Features.QRCode.Common;
using Tickette.Application.Features.QRCode.Queries.ValidateQrCode;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tickette.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : BaseController
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public OrdersController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [HttpPost("my-orders")]
    [Authorize]
    public async Task<ActionResult<ResponseDto<List<OrderedTicketGroupListDto>>>> Get([FromBody] ReviewOrdersQuery query, CancellationToken cancellation)
    {
        // Extract UserId from JWT token claims
        var userId = GetUserId();
        query.UserId = Guid.Parse(userId);

        // Dispatch the query
        var result = await _queryDispatcher.Dispatch<ReviewOrdersQuery, PagedResult<OrderedTicketGroupListDto>>(query, cancellation);
        var meta = new PaginationMeta(
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages);

        var response = ResponseHandler.PaginatedResponse(result.Items, meta, "Orders retrieved successfully");
        return Ok(response);
    }

    [HttpPost("validate-qrcode")]
    //[Authorize(Policy = "CheckInAccess")]
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
    public async Task<ResponseDto<CreateOrderResponse>> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken cancellation)
    {
        var userId = GetUserId();
        command.UserId = Guid.Parse(userId);

        var response = await _commandDispatcher.Dispatch<CreateOrderCommand, CreateOrderResponse>(command, cancellation);
        return ResponseHandler.SuccessResponse(response, "Order created successfully");
    }

    [HttpPost("reserve-tickets")]
    [SwaggerOperation(summary: "Reserve tickets for the user before confirm payment")]
    [Authorize]
    public async Task<ActionResult<ResponseDto<Unit>>> ReserveTickets([FromBody] ReserveTicketCommand command, CancellationToken cancellation)
    {
        var userId = GetUserId();
        command.UpdateUserId(Guid.Parse(userId));

        var response = await _commandDispatcher.Dispatch<ReserveTicketCommand, Unit>(command, cancellation);
        return Ok(ResponseHandler.SuccessResponse(response, "Tickets reserved successfully"));
    }

    [HttpPost("remove-tickets-reserve")]
    [SwaggerOperation(summary: "Remove the reservation of the tickets")]
    [Authorize]
    public async Task<ActionResult<ResponseDto<Unit>>>
        RemoveReserve([FromBody] RemoveReserveTicketCommand command, CancellationToken cancellation)
    {
        var userId = GetUserId();
        command.UpdateUserId(Guid.Parse(userId));

        var response = await _commandDispatcher.Dispatch<RemoveReserveTicketCommand, Unit>(command, cancellation);
        return Ok(ResponseHandler.SuccessResponse(response, "Reservation removed successfully"));
    }
}