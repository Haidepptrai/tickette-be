using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common.Admin;
using Tickette.Application.Wrappers;
using Tickette.Domain.Enums;

namespace Tickette.Application.Features.Events.Queries.Admin.GetAllEvents;

public record AdminGetAllEventsQuery
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }

    public ApprovalStatus? Status { get; init; }
}

public class GetAllEventsQueryHandler : IQueryHandler<AdminGetAllEventsQuery, PagedResult<AdminEventPreviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllEventsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminEventPreviewDto>> Handle(AdminGetAllEventsQuery request, CancellationToken cancellationToken)
    {
        // Build the query with related entities
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

        if (request.Status.HasValue)
        {
            query = query.Where(e => e.Status == request.Status.Value);
        }

        // Apply pagination
        var totalCount = await query.CountAsync(cancellationToken);

        var events = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map the entities to DTOs
        var eventDto = events.Select(e => e.ToEventPreviewDto()).ToList();

        var pageResult = new PagedResult<AdminEventPreviewDto>(eventDto, totalCount, request.PageNumber, request.PageSize);

        return pageResult;
    }
}