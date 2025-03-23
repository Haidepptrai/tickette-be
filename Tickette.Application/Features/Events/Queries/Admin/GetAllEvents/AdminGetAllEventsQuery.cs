using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common.Admin;
using Tickette.Application.Wrappers;

namespace Tickette.Application.Features.Events.Queries.Admin.GetAllEvents;

public record AdminGetAllEventsQuery
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
};

public class GetAllEventsQueryHandler : IQueryHandler<AdminGetAllEventsQuery, PagedResult<AdminEventPreviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllEventsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminEventPreviewDto>> Handle(AdminGetAllEventsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Events
            .Include(e => e.Category)
            .Include(e => e.Committee)
            .Include(e => e.EventDates)
            .ThenInclude(ed => ed.Tickets);

        // Apply pagination
        var totalCount = await query.CountAsync(cancellationToken);
        var events = await query
            .AsNoTracking()
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map the entities to DTOs
        var eventDto = events.Select(e => e.ToEventPreviewDto()).ToList();

        var pageResult = new PagedResult<AdminEventPreviewDto>(eventDto, totalCount, request.PageNumber, request.PageSize);

        return pageResult;
    }
}