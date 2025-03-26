using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.EventDates.Common;
using Tickette.Application.Features.EventDates.Query;
using Tickette.Application.Features.Seatmap.Command.SaveSeatMap;
using Tickette.Application.Features.Seatmap.Query;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;
using Tickette.Domain.Entities;

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

        [HttpPost("get-seat-map")]
        [SwaggerOperation("Get seat map of an event date by event date ID")]
        public async Task<ActionResult<ResponseDto<EventDateForSeatMapDto>>> GetEventDateSeatMap([FromBody] GetEventDateSeatMapQuery query, CancellationToken cancellationToken)
        {
            var eventDate = await _queryDispatcher.Dispatch<GetEventDateSeatMapQuery, EventSeatMap?>(query, cancellationToken);
            return Ok(ResponseHandler.SuccessResponse(eventDate, "Data Retrieve Successfully"));
        }
    }
}
