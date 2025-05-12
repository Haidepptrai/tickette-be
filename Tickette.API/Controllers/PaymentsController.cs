using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tickette.Application.Common;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Orders.Command.CreatePaymentIntent;
using Tickette.Application.Features.Orders.Command.UpdatePaymentIntent;
using Tickette.Application.Features.Orders.Common;
using Tickette.Application.Wrappers;

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public PaymentsController(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost("create-payment-intent")]
        [SwaggerOperation(summary: "Create payment intent for the user reserved the tickets")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _commandDispatcher.Dispatch<CreatePaymentIntentCommand, PaymentIntentResult>(command, cancellationToken);

            return Ok(ResponseHandler.SuccessResponse(result, "Payment Intent Created Successfully"));
        }

        [HttpPost("update-payment-intent")]
        [SwaggerOperation(summary: "Update payment pricing after user apply their coupon")]
        [Authorize]
        public async Task<ActionResult<ResponseDto<UpdatePaymentIntentResponse>>> UpdatePaymentTotalPrice([FromBody] UpdatePaymentIntentCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var result =
                    await _commandDispatcher.Dispatch<UpdatePaymentIntentCommand, UpdatePaymentIntentResponse>(command,
                        cancellationToken);
                return Ok(ResponseHandler.SuccessResponse(result, "Payment Total Price Updated Successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ResponseHandler.ErrorResponse<PaymentIntentResult>(null, ex.Message));
            }
        }

    }
}
