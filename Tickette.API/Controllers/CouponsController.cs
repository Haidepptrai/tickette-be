using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Coupons.Command.CreateCoupon;
using Tickette.Application.Features.Coupons.Common;
using Tickette.Application.Features.Coupons.Query.CalculateDiscountQuery;
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

        [HttpPost("create")]
        [SwaggerOperation(
            Summary = "Create Coupon",
            Description = "Create a new coupon for an event"
        )]
        //[Authorize(Policy = Constant.COMMITTEE_MEMBER_ROLES.EventOwner)]
        public async Task<ActionResult<ResponseDto<CreateCouponResponse>>> CreateCoupon([FromBody] CreateCouponCommand command, CancellationToken cancellation = default)
        {

            var response = await _commandDispatcher.Dispatch<CreateCouponCommand, ResponseDto<CreateCouponResponse>>(command, cancellation);

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
