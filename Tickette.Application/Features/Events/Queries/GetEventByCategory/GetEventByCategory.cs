using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;
using Tickette.Application.Wrappers;

namespace Tickette.Application.Features.Events.Queries.GetEventByCategory;

public record GetEventByCategory
{
    public string CategorySlug { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetEventByCategoryHandler : IQueryHandler<GetEventByCategory, PagedResult<EventPreviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEventByCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<EventPreviewDto>> Handle(GetEventByCategory query, CancellationToken cancellation)
    {
        var sqlQuery = _context.Events
            .Include(ev => ev.Category)
            .Include(ev => ev.Committee)
            .Include(ev => ev.EventDates)
            .ThenInclude(ed => ed.Tickets)
            .Where(ev => ev.Category.Name.ToLower().Contains(query.CategorySlug.ToLower()));

        var events = await sqlQuery
            .ToListAsync(cancellation);

        if (!events.Any())
        {
            throw new ValidationException("Category not found or no events exist in the category.");
        }

        var totalCount = events.Count;

        var pagedEvents = events
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => e.ToEventPreviewDto())
            .ToList();

        var result = new PagedResult<EventPreviewDto>(
            pagedEvents,
            totalCount,
            query.PageNumber,
            query.PageSize
        );

        return result;
    }
}