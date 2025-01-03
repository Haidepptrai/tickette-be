using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Orders.Common;
using Tickette.Application.Helpers;

namespace Tickette.Application.Features.Orders.Query.ReviewOrders;

public record ReviewOrdersQuery
{
    public required Guid UserId { get; set; }
}

public class ReviewOrdersQueryHandler : IQueryHandler<ReviewOrdersQuery, ResponseDto<List<OrderedTicketGroupListDto>>>
{
    private readonly IApplicationDbContext _context;

    public ReviewOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResponseDto<List<OrderedTicketGroupListDto>>> Handle(ReviewOrdersQuery query, CancellationToken cancellation)
    {
        try
        {
            var result = await _context.Orders
                .Where(o => o.UserOrderedId == query.UserId)
                .Include(o => o.Items)
                .SelectMany(o => o.Items)
                .GroupBy(ot => ot.OrderId) // Group by OrderId
                .Select(group => new OrderedTicketGroupListDto()
                {
                    OrderId = group.Key, // The OrderId key
                    Tickets = group.Select(ot => new OrderedTicketListDto
                    {
                        Id = ot.Id,
                        OrderId = ot.OrderId,
                        EventName = ot.Ticket.Event.Name,
                        VenueName = ot.Ticket.Event.Address,
                        TicketStartDate = ot.Ticket.Event.StartDate,
                        TicketEndDate = ot.Ticket.Event.EndDate
                    }).ToList()
                })
                .ToListAsync(cancellation);


            return ResponseHandler.SuccessResponse(result);
        }
        catch
        {
            throw new Exception("An error occurred while fetching the orders.");
        }
    }
}