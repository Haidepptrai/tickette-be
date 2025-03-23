using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common.Admin;
using Tickette.Application.Wrappers;

namespace Tickette.Application.Features.Events.Queries.Admin.SearchEventsByName;

public record SearchEventsByNameQuery
{
    public string Search { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}

public class SearchEventsByNameQueryHandler : IQueryHandler<SearchEventsByNameQuery, PagedResult<AdminEventPreviewDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchEventsByNameQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminEventPreviewDto>> Handle(SearchEventsByNameQuery query, CancellationToken cancellation)
    {
        var getAllEvents = _context.Events
            .Where(e => e.Name.ToLower().Contains(query.Search.ToLower()))
            .Include(e => e.Category)
            .Select(e => e.ToEventPreviewDto());

        var events = await getAllEvents
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync(cancellation);

        var totalCount = getAllEvents.Count();

        var pageResult = new PagedResult<AdminEventPreviewDto>(events, totalCount, query.PageNumber, query.PageSize);

        return pageResult;
    }
}