using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tickette.API.Dto;
using Tickette.API.Helpers;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Events.Commands.CreateEvent;
using Tickette.Application.Features.Events.Common;
using Tickette.Application.Features.Events.Queries.GetEventByCategory;
using Tickette.Application.Helpers;

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

        // GET api/<EventsController>/Category
        [HttpGet("{category:guid}")]
        public async Task<ResponseDto<IEnumerable<EventListDto>>> GetEventsByCategory(Guid category, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new GetEventByCategory
                {
                    CategoryId = category
                };

                var result = await _queryDispatcher.Dispatch<GetEventByCategory, IEnumerable<EventListDto>>(query, cancellationToken);
                var response = ResponseHandler.SuccessResponse(result, "Get all events successfully");
                return response;
            }
            catch (Exception ex)
            {
                return ResponseHandler.ErrorResponse<IEnumerable<EventListDto>>(null, "Internal Server Error", 500);
            }
        }

        // POST api/<EventsController>
        [HttpPost]
        [Authorize]
        public async Task<ResponseDto<Guid>> CreateEvent(
            CreateEventCommandDto commandDto, CancellationToken token)
        {
            try
            {
                // Extract the UserId from the JWT token
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return ResponseHandler.ErrorResponse(Guid.Empty, "User Id Not Found", 500);
                }

                var logoFile = new FormFileAdapter(commandDto.LogoFile);
                var bannerFile = new FormFileAdapter(commandDto.BannerFile);

                var command = new CreateEventCommand(
                    Guid.Parse(userId),
                    commandDto.Name,
                    commandDto.Address,
                    commandDto.CategoryId,
                    commandDto.Description,
                    commandDto.StartDate,
                    commandDto.EndDate,
                    commandDto.Committee,
                    commandDto.TicketInformation,
                    logoFile,
                    bannerFile
                );

                var response = await _commandDispatcher.Dispatch<CreateEventCommand, Guid>(command, token);
                return ResponseHandler.SuccessResponse(response, "Event created successfully");
            }
            catch (Exception ex)
            {
                return ResponseHandler.ErrorResponse(Guid.Empty, ex.Message, 500);
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
