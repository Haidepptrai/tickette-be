using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using Tickette.API.Dto;
using Tickette.API.Helpers;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Events.Commands.CreateEvent;
using Tickette.Application.Features.Events.Commands.UpdateEventStatus;
using Tickette.Application.Features.Events.Common;
using Tickette.Application.Features.Events.Queries.GetEventByCategory;
using Tickette.Application.Features.Events.Queries.GetEventById;
using Tickette.Application.Features.Events.Queries.GetEventByUserId;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;

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
    [SwaggerOperation(
        Summary = "Create Event",
        Description = "Create a new Event, default will have approval status is 0"
    )]
    public async Task<ResponseDto<Guid>> CreateEvent([FromForm] CreateEventCommandDto commandDto, CancellationToken token)
    {
        try
        {
            // Extract the UserId from the JWT token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
            {
                return ResponseHandler.ErrorResponse(Guid.Empty, "User ID not found in token", 500);
            }


            var logoFile = new FormFileAdapter(commandDto.LogoFile);
            var bannerFile = new FormFileAdapter(commandDto.BannerFile);

            var command = new CreateEventCommand(
                UserId: Guid.Parse(userIdClaim.Value),
                Name: commandDto.Name,
                LocationName: commandDto.LocationName,
                City: commandDto.City,
                District: commandDto.District,
                Ward: commandDto.Ward,
                StreetAddress: commandDto.StreetAddress,
                CategoryId: commandDto.CategoryId,
                Description: commandDto.Description,
                StartDate: commandDto.StartDate,
                EndDate: commandDto.EndDate,
                CommitteeName: commandDto.CommitteeName,
                CommitteeDescription: commandDto.CommitteeDescription,
                EventDatesInformation: commandDto.EventDates,
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

    // GET Event By User id
    [HttpPost("user")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Get Events By User Id",
        Description = "Get all events created by the user, userId provided in JWT"
    )]
    public async Task<ResponseDto<IEnumerable<EventListDto>>> GetEventsByUserId(
        CancellationToken token,
        int page = 1,
        int perPage = 10
        )
    {

        try
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
            {
                return ResponseHandler.ErrorResponse<IEnumerable<EventListDto>>(null, "User ID not found in token", 500);
            }

            var query = new GetEventByUserIdQuery(Guid.Parse(userIdClaim.Value));

            var result = await _queryDispatcher.Dispatch<GetEventByUserIdQuery, IEnumerable<EventListDto>>(query, token);

            var totalItems = result.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / perPage);

            var paginatedData = result
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            var paginationMeta = new PaginationMeta(page, perPage, totalItems, totalPages);


            var response = ResponseHandler.PaginatedResponse(paginatedData, paginationMeta, "Get events successfully");
            return response;
        }
        catch (Exception ex)
        {
            return ResponseHandler.ErrorResponse<IEnumerable<EventListDto>>(null, "Internal Server Error", 500);
        }
    }

    //Update Event Status
    [HttpPatch("{eventId:guid}/status")]
    [Authorize(Roles = Constant.APPLICATION_ROLE.Admin)]
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