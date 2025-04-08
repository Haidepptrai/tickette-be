using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tickette.API.Dto;
using Tickette.API.Helpers;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Events.Commands.CreateEvent;
using Tickette.Application.Features.Events.Commands.UpdateEventStatus;
using Tickette.Application.Features.Events.Common;
using Tickette.Application.Features.Events.Common.Client;
using Tickette.Application.Features.Events.Queries.Client.GetEventByUserId;
using Tickette.Application.Features.Events.Queries.Client.GetEventDetailStatistic;
using Tickette.Application.Features.Events.Queries.Client.GetSeatsOrderedInfo;
using Tickette.Application.Features.Events.Queries.GetAllEvents;
using Tickette.Application.Features.Events.Queries.GetEventByCategory;
using Tickette.Application.Features.Events.Queries.GetEventById;
using Tickette.Application.Features.Events.Queries.GetEventBySlug;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common;

namespace Tickette.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : BaseController
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
        var query = new GetEventByCategory(categoryId);
        var result = await _queryDispatcher.Dispatch<GetEventByCategory, IEnumerable<EventListDto>>(query, cancellationToken);
        var response = ResponseHandler.SuccessResponse(result, "Get all events successfully");
        return response;
    }

    // GET all events
    [HttpPost("get-all")]
    [SwaggerOperation(
        Summary = "Get All Events",
        Description = "Get all events with pagination"
    )]
    public async Task<ActionResult<ResponseDto<IEnumerable<EventPreviewDto>>>> GetAllEvents(GetAllEventsQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _queryDispatcher.Dispatch<GetAllEventsQuery, PagedResult<EventPreviewDto>>(query, cancellationToken);

            var paginationMeta = new PaginationMeta(
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                (int)Math.Ceiling((double)result.TotalCount / result.PageSize)
            );

            var response = ResponseHandler.PaginatedResponse(result.Items, paginationMeta, "Get all events successfully");

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ResponseHandler.ErrorResponse(Unit.Value, ex.Message, 500));
        }
    }

    [HttpPost("id")]
    [SwaggerOperation("Get event detail by id")]
    public async Task<ResponseDto<EventDetailDto>> GetEventById(GetEventByIdRequest query, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        query.UserId = Guid.Parse(userId);

        var result = await _queryDispatcher.Dispatch<GetEventByIdRequest, EventDetailDto>(query, cancellationToken);
        var response = ResponseHandler.SuccessResponse(result, "Get event by id successfully");
        return response;
    }

    // GET event by slug
    [HttpPost("get-by-slug")]
    public async Task<ActionResult<ResponseDto<EventDetailDto>>> GetEventBySlug(GetEventBySlugQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _queryDispatcher.Dispatch<GetEventBySlugQuery, EventDetailDto>(query, cancellationToken);
            var response = ResponseHandler.SuccessResponse(result, "Get event by slug successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ResponseHandler.ErrorResponse<EventDetailDto>(null, "Internal Server Error", 500));
        }
    }

    [HttpPost]
    [Authorize]
    [SwaggerOperation(
        Summary = "Create Event",
        Description = "Create a new Event, default will have approval status is 0"
    )]
    public async Task<ResponseDto<Guid>> CreateEvent([FromForm] CreateEventCommandDto commandDto, CancellationToken token)
    {
        // Extract the UserId from the JWT token
        var userId = GetUserId();

        var bannerFile = new FormFileAdapter(commandDto.BannerFile);
        var committeeLogo = new FormFileAdapter(commandDto.CommitteeLogo);

        var command = new CreateEventCommand(
            UserId: Guid.Parse(userId),
            Name: commandDto.Name,
            LocationName: commandDto.LocationName,
            City: commandDto.City,
            District: commandDto.District,
            Ward: commandDto.Ward,
            StreetAddress: commandDto.StreetAddress,
            CategoryId: commandDto.CategoryId,
            Description: commandDto.Description,
            CommitteeLogo: committeeLogo,
            CommitteeName: commandDto.CommitteeName,
            CommitteeDescription: commandDto.CommitteeDescription,
            IsOffline: commandDto.IsOffline,
            EventDatesInformation: commandDto.EventDates,
            bannerFile,
            EventOwnerStripeId: commandDto.EventOwnerStripeId
        );

        var response = await _commandDispatcher.Dispatch<CreateEventCommand, Guid>(command, token);
        return ResponseHandler.SuccessResponse(response, "Event created successfully");
    }

    // GET Event By User id
    [HttpPost("user")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Get Events By User Id",
        Description = "Get all events created by the user, userId provided in JWT"
    )]
    public async Task<ActionResult<ResponseDto<IEnumerable<UserEventListResponse>>>> GetEventsByUserId(
        CancellationToken token,
        GetEventByUserIdQuery query)
    {
        var userId = GetUserId();

        query.UserId = Guid.Parse(userId);

        var result = await _queryDispatcher.Dispatch<GetEventByUserIdQuery, PagedResult<UserEventListResponse>>(query, token);

        var paginationMeta = new PaginationMeta(result.PageNumber, result.PageSize, result.TotalCount, result.TotalPages);

        var response = ResponseHandler.PaginatedResponse(result.Items, paginationMeta, "Get events successfully");
        return Ok(response);
    }

    //Update Event Status
    [HttpPatch("status")]
    //[Authorize(Roles = Constant.APPLICATION_ROLE.Admin)]
    public async Task<ResponseDto<Guid>> UpdateEventStatus(UpdateEventStatusCommand command, CancellationToken token)
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

    [HttpPost("detail-statistic")]
    [SwaggerOperation("Get event detail statistic")]
    public async Task<ResponseDto<EventDetailStatisticDto>> GetEventDetailStatistic(GetEventDetailStatisticQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _queryDispatcher.Dispatch<GetEventDetailStatisticQuery, EventDetailStatisticDto>(query, cancellationToken);
        var response = ResponseHandler.SuccessResponse(result, "Get event detail statistic successfully");
        return response;
    }

    [HttpPost("get-seats-ordered-info")]
    [SwaggerOperation("Get seats ordered info for marking it in seat map")]
    public async Task<ActionResult<ResponseDto<IEnumerable<SeatsOrderedInfoDto>>>> GetSeatsOrderedInfo(GetSeatsOrderedInfoQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _queryDispatcher.Dispatch<GetSeatsOrderedInfoQuery, IEnumerable<SeatsOrderedInfoDto>>(query, cancellationToken);
        var response = ResponseHandler.SuccessResponse(result, "Get seats ordered info successfully");
        return response;
    }
}