using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Orders.Common;
using Tickette.Application.Features.Orders.Query.ReviewOrders;
using Tickette.Application.Features.QRCode.Common;
using Tickette.Application.Features.QRCode.Queries.AdminCheckQrCodeFraud;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;

namespace Tickette.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;

        public OrdersController(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [HttpPost("user/order-history")]
        [SwaggerOperation("Get all orders of a specific user")]
        [Authorize(Roles = $"{Constant.APPLICATION_ROLE.Admin},{Constant.APPLICATION_ROLE.Moderator}")]
        public async Task<ActionResult<ResponseDto<List<OrderedTicketGroupListDto>>>> Get([FromBody] ReviewOrdersQuery query, CancellationToken cancellation)
        {
            var result = await _queryDispatcher.Dispatch<ReviewOrdersQuery, PagedResult<OrderedTicketGroupListDto>>(query, cancellation);

            var meta = new PaginationMeta(
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                result.TotalPages);

            var response = ResponseHandler.PaginatedResponse(result.Items, meta, "Orders retrieved successfully");
            return Ok(response);
        }

        [HttpPost("qr-code-check")]
        [SwaggerOperation("Check QR code fraud")]
        [Authorize(Roles = $"{Constant.APPLICATION_ROLE.Admin},{Constant.APPLICATION_ROLE.Moderator}")]
        public async Task<ActionResult<ResponseDto<DataRetrievedFromQrCode>>> CheckQrCode([FromBody] AdminCheckQrCodeFraudQuery query, CancellationToken cancellation)
        {
            var result = await _queryDispatcher.Dispatch<AdminCheckQrCodeFraudQuery, ResponseDto<DataRetrievedFromQrCode>>(query, cancellation);
            return Ok(result);
        }
    }
}
