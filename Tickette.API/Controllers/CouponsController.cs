using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Coupons.Command.Client.CreateCoupon;
using Tickette.Application.Features.Coupons.Command.Client.DeleteCoupon;
using Tickette.Application.Features.Coupons.Command.Client.UpdateCoupon;
using Tickette.Application.Features.Coupons.Common;
using Tickette.Application.Features.Coupons.Query.CalculateDiscountQuery;
using Tickette.Application.Features.Coupons.Query.Client.GetCouponList;
using Tickette.Application.Wrappers;

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;

        public CouponsController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        [HttpPost("list")]
        [SwaggerOperation(
            Summary = "List Coupons",
            Description = "List all coupons for an event, NO PAGINATION since it assume a small list of data"
        )]
        public async Task<ActionResult<ResponseDto<IEnumerable<CouponResponse>>>> GetCouponList([FromBody] GetCouponListQuery query, CancellationToken cancellation = default)
        {
            var data = await _queryDispatcher.Dispatch<GetCouponListQuery, IEnumerable<CouponResponse>>(query, cancellation);
            var response = ResponseHandler.SuccessResponse(data, "Get Coupon List Successfully");

            return Ok(response);
        }

        [HttpPost("create")]
        [SwaggerOperation(
            Summary = "Create Coupon",
            Description = "Create a new coupon for an event"
        )]
        //[Authorize(Policy = Constant.COMMITTEE_MEMBER_ROLES.EventOwner)]
        public async Task<ActionResult<ResponseDto<CouponResponse>>> CreateCoupon([FromBody] CreateCouponCommand command, CancellationToken cancellation = default)
        {

            var response = await _commandDispatcher.Dispatch<CreateCouponCommand, ResponseDto<CouponResponse>>(command, cancellation);

            return Ok(response);
        }

        [HttpPost("update")]
        [SwaggerOperation(
            Summary = "Update Coupon",
            Description = "Update an existing coupon for an event"
        )]
        public async Task<ActionResult<ResponseDto<CouponResponse>>> UpdateCoupon([FromBody] UpdateCouponCommand command, CancellationToken cancellation = default)
        {
            var data = await _commandDispatcher.Dispatch<UpdateCouponCommand, CouponResponse>(command, cancellation);
            var response = ResponseHandler.SuccessResponse(data, "Update Coupon Successfully");

            return Ok(response);
        }

        [HttpPost("delete")]
        [SwaggerOperation(
            Summary = "Delete Coupon",
            Description = "Delete an existing coupon for an event"
        )]
        public async Task<ActionResult<ResponseDto<bool>>> DeleteCoupon([FromBody] DeleteCouponCommand command, CancellationToken cancellation = default)
        {
            var data = await _commandDispatcher.Dispatch<DeleteCouponCommand, bool>(command, cancellation);
            if (!data) throw new Exception("Internal Server Error");

            var response = ResponseHandler.SuccessResponse(data, "Delete Coupon Successfully");
            return Ok(response);
        }

        [HttpPost("calculate-discount")]
        [SwaggerOperation(
            Summary = "Calculate Discount",
            Description = "Calculate discount based on the given coupon code"
        )]
        [Authorize(Policy = "EventOwnerPolicy")]
        public async Task<IActionResult> CalculateDiscount([FromBody] CalculateDiscountQuery query, CancellationToken cancellation = default)
        {
            try
            {
                var response = await _queryDispatcher.Dispatch<CalculateDiscountQuery, ResponseDto<PriceDiscountInformationDto>>(query, cancellation);
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHandler.ErrorResponse<PriceDiscountInformationDto>(null, ex.Message));
            }
        }
    }
}
