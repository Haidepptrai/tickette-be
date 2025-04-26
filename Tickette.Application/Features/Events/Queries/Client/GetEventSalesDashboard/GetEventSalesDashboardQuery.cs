using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common.Client;

namespace Tickette.Application.Features.Events.Queries.Client.GetEventSalesDashboard;

public record GetEventSalesDashboardQuery
{
    public Guid EventId { get; init; }

    public string TimeRange { get; init; } = "all"; // "today", "week", "month", "all"
}

public class GetEventSalesDashboardQueryHandler : IQueryHandler<GetEventSalesDashboardQuery, EventSalesDashboardDto>
{
    private readonly IApplicationDbContext _context;

    public GetEventSalesDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventSalesDashboardDto> Handle(GetEventSalesDashboardQuery query, CancellationToken cancellationToken)
    {
        var eventEntity = await _context.Events
            .Include(e => e.EventDates)
                .ThenInclude(ed => ed.Tickets)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == query.EventId, cancellationToken);

        if (eventEntity == null)
            throw new KeyNotFoundException($"Event with ID {query.EventId} was not found.");

        // Get the event's currency from the first ticket's price currency
        var currency = eventEntity.EventDates
            .SelectMany(ed => ed.Tickets)
            .FirstOrDefault()?.Price.Currency ?? "USD";

        // Filter orders by time range
        var startDate = GetStartDateFromTimeRange(query.TimeRange);

        // Get all confirmed orders for this event
        var orderItems = await _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Ticket)
            .Where(oi => oi.Ticket.EventDate.EventId == query.EventId
                && (startDate == null || oi.Order.CreatedAt >= startDate))
            .ToListAsync(cancellationToken);

        // Calculate total revenue and tickets sold
        var totalRevenue = orderItems.Sum(oi => oi.Price.Amount);
        var totalTicketsSold = orderItems.Sum(oi => oi.Quantity);

        // Group sales by ticket type
        var ticketTypeSales = orderItems
            .GroupBy(oi => new { oi.TicketId, oi.Ticket.Name })
            .Select(g => new TicketTypeSalesDto
            {
                TicketTypeId = g.Key.TicketId,
                TicketTypeName = g.Key.Name,
                QuantitySold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.Price.Amount)
            })
            .ToList();

        return new EventSalesDashboardDto
        {
            EventId = eventEntity.Id,
            EventName = eventEntity.Name,
            TotalRevenue = totalRevenue,
            Currency = currency,
            TotalTicketsSold = totalTicketsSold,
            TicketTypeSales = ticketTypeSales
        };
    }

    private DateTime? GetStartDateFromTimeRange(string timeRange)
    {
        var now = DateTime.UtcNow;

        return timeRange.ToLower() switch
        {
            "today" => DateTime.UtcNow.Date,
            "week" => DateTime.UtcNow.AddDays(-7),
            "month" => DateTime.UtcNow.AddMonths(-1),
            _ => null // "all" time or invalid value
        };
    }
}