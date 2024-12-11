using Microsoft.AspNetCore.Mvc;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Events.Commands.CreateEvent;
using Tickette.Application.Events.Common;
using Tickette.Application.Events.Queries;
using Tickette.Application.Helpers;
using Tickette.Domain.Enums;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tickette.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ICommandDispatcher _commandDispatcher;

        public EventsController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
        }

        // GET: api/<EventsController>
        [HttpGet]
        public async Task<ResponseDto<IEnumerable<EventListDto>>> GetAllEvents(CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new GetEventByEventType
                {
                    Type = EventType.Concert
                };

                var result = await _queryDispatcher.Dispatch<GetEventByEventType, IEnumerable<EventListDto>>(query, cancellationToken);
                var response = ResponseHandler.SuccessResponse(result, "Get all events successfully");
                return response;
            }
            catch (Exception ex)
            {
                return ResponseHandler.ErrorResponse<IEnumerable<EventListDto>>(null, "Internal Server Error", 500);
            }
        }

        // GET api/<EventsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<EventsController>
        [HttpPost]
        public async Task<ResponseDto<Guid>> CreateEvent([FromBody] CreateEventCommand command, CancellationToken token)
        {
            try
            {
                var response = await _commandDispatcher.Dispatch<CreateEventCommand, Guid>(command, token);
                return ResponseHandler.SuccessResponse(response, "Event created successfully");
            }
            catch
            {
                return ResponseHandler.ErrorResponse(Guid.Empty, "Internal Server Error", 500);
            }
        }

        // PUT api/<EventsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<EventsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
