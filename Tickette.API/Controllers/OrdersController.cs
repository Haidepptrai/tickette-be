using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;
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

        public OrdersController(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
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
