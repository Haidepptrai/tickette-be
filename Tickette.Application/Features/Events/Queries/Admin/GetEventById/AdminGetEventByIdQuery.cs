using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;
using Tickette.Application.Features.Events.Common;

namespace Tickette.Application.Features.Events.Queries.Admin.GetEventById;

public record AdminGetEventByIdQuery
{
    public Guid Id { get; init; }
}

public class AdminGetEventByIdRequestHandler : IQueryHandler<AdminGetEventByIdQuery, EventDetailDto>
{
    private readonly IApplicationDbContext _context;

    public AdminGetEventByIdRequestHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventDetailDto> Handle(AdminGetEventByIdQuery query, CancellationToken cancellation)
    {
        var result = await _context.Events
            .Include(ev => ev.Category)
            .Include(ev => ev.Committee)
            .Include(ev => ev.EventDates)
            .ThenInclude(ed => ed.Tickets)
            .AsSplitQuery()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(ev => ev.Id == query.Id, cancellation);

        if (result == null)
            throw new NotFoundException("Event", query.Id);

        var categoryList = await _context.Categories
            .AsNoTracking()
            .ToListAsync(cancellation);

        if (categoryList == null)
            throw new Exception("An error has occurred");

        var resultDto = result.ToEventDetailDto(categoryList, null);

        return resultDto;
    }
}