﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;
using Tickette.Application.Wrappers;

namespace Tickette.Application.Features.Events.Queries.GetAllEvents;

public record GetAllEventsQuery
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; } = null;
};

public class GetAllEventsQueryHandler : IQueryHandler<GetAllEventsQuery, PagedResult<EventPreviewDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetAllEventsQueryHandler> _logger;

    public GetAllEventsQueryHandler(IApplicationDbContext context, ILogger<GetAllEventsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<EventPreviewDto>> Handle(GetAllEventsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Committee)
                .Include(e => e.EventDates)
                .ThenInclude(ed => ed.Tickets)
                .AsNoTracking();

            // Apply filtering if a search term is provided
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var loweredSearch = request.Search.ToLower();
                query = query.Where(e => e.Name.ToLower().Contains(loweredSearch));
            }

            // Apply pagination
            var totalCount = await query.CountAsync(cancellationToken);
            var events = await query
                .AsNoTracking()
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Map the entities to DTOs
            var eventDto = events.Select(e => e.ToEventPreviewDto()).ToList();

            var pageResult = new PagedResult<EventPreviewDto>(eventDto, totalCount, request.PageNumber, request.PageSize);

            return pageResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching events.");
            throw new ApplicationException("An error has occurred while get all events query");
        }
    }
}