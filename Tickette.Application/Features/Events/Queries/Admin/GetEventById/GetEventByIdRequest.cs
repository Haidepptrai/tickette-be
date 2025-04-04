using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;

namespace Tickette.Application.Features.Events.Queries.Admin.GetEventById;

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
        try
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
                throw new KeyNotFoundException($"Event with ID {query.Id} was not found.");

            var category = await _context.Categories
                .AsNoTracking()
                .ToListAsync(cancellation);

            var resultDto = result.ToEventDetailDto(category);

            return resultDto;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            throw new ApplicationException($"An error occurred while retrieving the event with ID {query.Id}.", ex);
        }
    }
}