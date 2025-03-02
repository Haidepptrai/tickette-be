using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.EventDates.Common;
using Tickette.Application.Features.EventDates.Query;
using Tickette.Application.Features.Seatmap.SaveSeatMap;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventSeatMapsController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;

        public EventSeatMapsController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        [HttpPost("save")]
        public async Task<ActionResult<ResponseDto<Unit>>> Post([FromBody] SaveSeatMapCommand command, CancellationToken cancellationToken)
        {
            await _commandDispatcher.Dispatch<SaveSeatMapCommand, Unit>(command, cancellationToken);

            return Ok(ResponseHandler.SuccessResponse(Unit.Value, "Seat map saved successfully"));
        }

        [HttpPost("get-dates-information")]
        public async Task<ActionResult<ResponseDto<IEnumerable<EventDateForSeatMapDto>>>> GetEventDates([FromBody] GetEventDatesQuery query, CancellationToken cancellationToken)
        {
            var eventDates = await _queryDispatcher.Dispatch<GetEventDatesQuery, IEnumerable<EventDateForSeatMapDto>>(query, cancellationToken);
            return Ok(ResponseHandler.SuccessResponse(eventDates, "Data Retrieve Successfully"));
        }
    }
}
