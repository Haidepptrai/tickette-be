using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Coupons.Command.CreateCoupon;
using Tickette.Application.Helpers;

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public CouponsController(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponCommand command, CancellationToken cancellation = default)
        {
            try
            {
                var response = await _commandDispatcher.Dispatch<CreateCouponCommand, ResponseDto<CreateCouponResponse>>(command, cancellation);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHandler.ErrorResponse<CreateCouponResponse>(null, ex.Message));
            }
        }
    }
}
