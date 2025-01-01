using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Tickets.Command;
using Tickette.Application.Features.Tickets.Common;
using Tickette.Application.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderTicketsController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public OrderTicketsController(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost]
        public async Task<ResponseDto<ResponseTicketOrderedDto>> Post([FromBody] OrderTicketsCommand command, CancellationToken cancellation)
        {
            try
            {
                var response =
                    await _commandDispatcher.Dispatch<OrderTicketsCommand, ResponseTicketOrderedDto>(command, cancellation);
                return ResponseHandler.SuccessResponse(response, "Ticket ordered successfully");
            }
            catch
            {
                return ResponseHandler.ErrorResponse<ResponseTicketOrderedDto>(null, "Failed to order ticket");
            }
        }
    }
}
