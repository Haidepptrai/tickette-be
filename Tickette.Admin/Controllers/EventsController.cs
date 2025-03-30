using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Features.Events.Commands.UpdateEventStatus;
using Tickette.Application.Features.Events.Common;
using Tickette.Application.Features.Events.Common.Admin;
using Tickette.Application.Features.Events.Queries.Admin.GetAllEvents;
using Tickette.Application.Features.Events.Queries.Admin.GetEventsStatistic;
using Tickette.Application.Features.Events.Queries.Admin.SearchEventsByName;
using Tickette.Application.Features.Events.Queries.Client.GetEventByUserId;
using Tickette.Application.Features.Events.Queries.GetEventByCategory;
using Tickette.Application.Features.Events.Queries.GetEventById;
using Tickette.Application.Wrappers;

namespace Tickette.Admin.Controllers;

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

    // GET all events
    [HttpPost("get-all")]
    [SwaggerOperation(
        Summary = "Get All Events",
        Description = "Get all events with pagination"
    )]
    public async Task<ResponseDto<IEnumerable<AdminEventPreviewDto>>> GetAllEvents(AdminGetAllEventsQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _queryDispatcher.Dispatch<AdminGetAllEventsQuery, PagedResult<AdminEventPreviewDto>>(query, cancellationToken);

        var paginationMeta = new PaginationMeta(
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages
        );

        var response = ResponseHandler.PaginatedResponse(result.Items, paginationMeta, "Get all events successfully");

        return response;
    }

    // GET event by id
    [HttpPost("id")]
    public async Task<ResponseDto<EventDetailDto>> GetEventById(GetEventByIdRequest query, CancellationToken cancellationToken = default)
    {
        var result = await _queryDispatcher.Dispatch<GetEventByIdRequest, EventDetailDto>(query, cancellationToken);
        var response = ResponseHandler.SuccessResponse(result, "Get event by id successfully");
        return response;
    }

    // GET Event By User id
    [HttpPost("user")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Get Events By User Id",
        Description = "Get all events created by the user, user id in body for admin"
    )]
    public async Task<ResponseDto<IEnumerable<EventListDto>>> GetEventsByUserId(
        GetEventByUserIdQuery query,
        CancellationToken token,
        int page = 1,
        int perPage = 10
        )
    {

        try
        {
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

    [HttpPost("search")]
    [SwaggerOperation(
        Summary = "Search Events",
        Description = "Search events by title"
    )]
    public async Task<ResponseDto<IEnumerable<Application.Features.Events.Common.Admin.AdminEventPreviewDto>>> SearchEvents(SearchEventsByNameQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _queryDispatcher.Dispatch<SearchEventsByNameQuery, PagedResult<Application.Features.Events.Common.Admin.AdminEventPreviewDto>>(query, cancellationToken);
        var meta = new PaginationMeta(result.PageNumber, result.PageSize, result.TotalCount, result.TotalPages);

        var response = ResponseHandler.PaginatedResponse(result.Items, meta, "Search events successfully");
        return response;
    }

    //Update Event Status
    [HttpPost("status")]
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

    [HttpPost("statistic")]
    [SwaggerOperation(Summary = "Get all statistic counting of events", Description = "Get all counting of events for admin dashboard, include pending, approved, denied, upcoming (next 7 days). All event statistic within one month")]
    public async Task<ResponseDto<EventsStatisticDto>> GetEventStatistic(CancellationToken token)
    {
        var query = new GetEventsStatisticQuery();
        var result = await _queryDispatcher.Dispatch<GetEventsStatisticQuery, EventsStatisticDto>(query, token);
        return ResponseHandler.SuccessResponse(result, "Get event statistic successfully");
    }
}