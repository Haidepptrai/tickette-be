using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;

namespace Tickette.Application.Features.Events.Queries.GetEventBySlug;

public record GetEventBySlugQuery(string Slug);

public class GetEventBySlugQueryHandler : IQueryHandler<GetEventBySlugQuery, EventDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetEventBySlugQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventDetailDto> Handle(GetEventBySlugQuery query, CancellationToken cancellation)
    {
        try
        {
            var result = await _context.Events
                .Include(ev => ev.Category)
                .Include(ev => ev.Committee)
                .Include(ev => ev.EventDates)
                .ThenInclude(ed => ed.Tickets)
                .AsSplitQuery()
                .AsNoTracking()
                .SingleOrDefaultAsync(ev => ev.EventSlug == query.Slug, cancellation);

            if (result == null)
                throw new KeyNotFoundException($"Event with ID {query.Slug} was not found.");

            var resultDto = result.ToEventDetailDto(null, null);

            return resultDto;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            throw new ApplicationException($"An error occurred while retrieving the event with ID {query.Slug}.", ex);
        }

    }
}