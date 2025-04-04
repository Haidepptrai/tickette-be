using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Exceptions;
using Tickette.Application.Features.Events.Common;

namespace Tickette.Application.Features.Events.Queries.GetEventById;

public record GetEventByIdRequest
{
    public Guid Id { get; init; }
}

public class GetEventByIdHandler : IQueryHandler<GetEventByIdRequest, EventDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetEventByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventDetailDto> Handle(GetEventByIdRequest query, CancellationToken cancellation)
    {
        var result = await _context.Events
       .Include(ev => ev.Category)
       .Include(ev => ev.Committee)
       .Include(ev => ev.EventDates)
           .ThenInclude(ed => ed.Tickets)
       .AsSplitQuery()
       .AsNoTracking()
       .SingleOrDefaultAsync(ev => ev.Id == query.Id, cancellation);

        if (result == null)
            throw new NotFoundException("Event", query.Id);

        var category = await _context.Categories
            .AsNoTracking()
            .ToListAsync(cancellation);

        var resultDto = result.ToEventDetailDto(category);

        return resultDto;
    }
}