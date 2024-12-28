using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tickette.API.Dto;
using Tickette.API.Helpers;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Events.Commands.CreateEvent;
using Tickette.Application.Features.Events.Commands.UpdateEventStatus;
using Tickette.Application.Features.Events.Common;
using Tickette.Application.Features.Events.Queries.GetEventByCategory;
using Tickette.Application.Features.Events.Queries.GetEventById;
using Tickette.Application.Helpers;

namespace Tickette.API.Controllers;

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

    // GET event by category id
    [HttpGet("categories/{categoryId:guid}")]
    public async Task<ResponseDto<IEnumerable<EventListDto>>> GetEventsByCategory(Guid categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetEventByCategory(categoryId);
            var result = await _queryDispatcher.Dispatch<GetEventByCategory, IEnumerable<EventListDto>>(query, cancellationToken);
            var response = ResponseHandler.SuccessResponse(result, "Get all events successfully");
            return response;
        }
        catch (Exception ex)
        {
            return ResponseHandler.ErrorResponse<IEnumerable<EventListDto>>(null, "Internal Server Error", 500);
        }
    }

    // GET event by id
    [HttpGet("{id:guid}")]
    public async Task<ResponseDto<EventDetailDto>> GetEventById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetEventByIdRequest(id);
            var result = await _queryDispatcher.Dispatch<GetEventByIdRequest, EventDetailDto>(query, cancellationToken);
            var response = ResponseHandler.SuccessResponse(result, "Get event by id successfully");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return ResponseHandler.ErrorResponse<EventDetailDto>(null, "Internal Server Error", 500);
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

    //Update Event Status
    [HttpPut("{eventId:guid}/status")]
    [Authorize]
    public async Task<ResponseDto<Guid>> UpdateEventStatus(Guid eventId, UpdateEventStatusCommand command, CancellationToken token)
    {
        try
        {
            var response = await _commandDispatcher.Dispatch<UpdateEventStatusCommand, Guid>(command, token);
            return ResponseHandler.SuccessResponse(response, "Event status updated successfully");
        }
        catch (Exception ex)
        {
            return ResponseHandler.ErrorResponse(Guid.Empty, ex.Message, 500);
        }
    }

}