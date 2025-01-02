using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Tickette.Application.Common;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;

namespace Tickette.Application.Features.Events.Queries.GetEventByCategory;

public record GetEventByCategory(Guid CategoryId);

public class GetEventByCategoryHandler : BaseHandler<GetEventByCategoryHandler>, IQueryHandler<GetEventByCategory, IEnumerable<EventListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEventByCategoryHandler(IApplicationDbContext context, ILogger<GetEventByCategoryHandler> logger) : base(logger)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventListDto>> Handle(GetEventByCategory query, CancellationToken cancellation)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var events = await _context.Events
                .Include(ev => ev.Category)
                .Include(ev => ev.Committee)
                .Include(ev => ev.Tickets)
                .Where(c => c.CategoryId == query.CategoryId)
                .ToListAsync(cancellation);

            if (!events.Any())
            {
                throw new ValidationException("Category not found or no events exist in the category.");
            }

            var eventsDto = events.Select(e => e.ToEventListDto()).ToList();
            return eventsDto;
        }, "Get Event By Category");
    }
}