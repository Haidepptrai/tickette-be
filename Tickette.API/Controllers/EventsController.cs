using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tickette.API.DTOs;
using Tickette.API.Helpers;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Events.Commands.CreateEvent;
using Tickette.Application.Features.Events.Commands.UpdateEvent;
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
    [HttpPost("category")]
    [SwaggerOperation("Get all events by a specific category")]
    public async Task<ResponseDto<IEnumerable<EventPreviewDto>>> GetEventsByCategory(
        [FromBody] GetEventByCategory query,
        CancellationToken cancellationToken = default)
    {
        var result = await _queryDispatcher.Dispatch<GetEventByCategory, PagedResult<EventPreviewDto>>(query, cancellationToken);

        var meta = new PaginationMeta(
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages);
        var response = ResponseHandler.PaginatedResponse(result.Items, meta, "Get all events by category successfully");
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
    [HttpPost("slug")]
    [SwaggerOperation("Get event detail by slug")]
    public async Task<ActionResult<ResponseDto<EventDetailDto>>> GetEventBySlug(GetEventBySlugQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _queryDispatcher.Dispatch<GetEventBySlugQuery, EventDetailDto>(query, cancellationToken);
        var response = ResponseHandler.SuccessResponse(result, "Get event by slug successfully");
        return Ok(response);
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

        var bannerFile = new FormFileAdapter(commandDto.Banner);
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

    [HttpPost("update")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Update Event",
        Description = "Update an existing Event, default will have approval status is 0"
    )]
    public async Task<ResponseDto<Guid>> UpdateEvent([FromForm] UpdateEventCommandDto commandDto, CancellationToken token)
    {
        // Extract the UserId from the JWT token
        var userId = GetUserId();

        var bannerFile = commandDto.BannerFile != null ? new FormFileAdapter(commandDto.BannerFile) : null;
        var committeeLogo = commandDto.CommitteeLogo != null ? new FormFileAdapter(commandDto.CommitteeLogo) : null;

        var bannerUrl = commandDto.BannerUrl;
        var committeeLogoUrl = commandDto.CommitteeLogoUrl;

        var command = new UpdateEventCommand(
            Id: commandDto.Id,
            UserId: Guid.Parse(userId),
            Name: commandDto.Name,
            LocationName: commandDto.IsOffline ? commandDto.LocationName! : string.Empty,
            City: commandDto.IsOffline ? commandDto.City! : string.Empty,
            District: commandDto.IsOffline ? commandDto.District! : string.Empty,
            Ward: commandDto.IsOffline ? commandDto.Ward! : string.Empty,
            StreetAddress: commandDto.IsOffline ? commandDto.StreetAddress! : string.Empty,
            CategoryId: commandDto.CategoryId,
            Description: commandDto.Description,
            CommitteeLogo: committeeLogo,
            CommitteeLogoUrl: committeeLogoUrl,
            CommitteeName: commandDto.CommitteeName,
            CommitteeDescription: commandDto.CommitteeDescription,
            IsOffline: commandDto.IsOffline,
            EventDatesInformation: commandDto.EventDates,
            bannerFile,
            BannerUrl: bannerUrl,
            EventOwnerStripeId: commandDto.EventOwnerStripeId
        );

        var response = await _commandDispatcher.Dispatch<UpdateEventCommand, Guid>(command, token);
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