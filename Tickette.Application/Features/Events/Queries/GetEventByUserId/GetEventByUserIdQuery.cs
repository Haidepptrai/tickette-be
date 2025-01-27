using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common;

namespace Tickette.Application.Features.Events.Queries.GetEventByUserId;

public record GetEventByUserIdQuery(Guid UserId);

public class GetEventByUserIdQueryHandler : IQueryHandler<GetEventByUserIdQuery, IEnumerable<EventListDto>>
{
    private readonly IApplicationDbContext _context;


    public GetEventByUserIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<EventListDto>> Handle(GetEventByUserIdQuery request, CancellationToken cancellationToken)
    {
        var events = await _context.Events
            .IgnoreQueryFilters()
            .Where(e => e.User.Id == request.UserId)
            .Include(e => e.Category)
            .Include(e => e.Committee)
            .ToListAsync(cancellationToken);


        var eventsToListDto = events.Select(e => e.ToEventListDto()).ToList();

        return eventsToListDto;
    }
}