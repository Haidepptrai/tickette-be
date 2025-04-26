using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Events.Common.Client;
using Tickette.Application.Wrappers;

namespace Tickette.Application.Features.Events.Queries.Client.GetRecentPurchases;

public record GetRecentPurchasesQuery
{
    public Guid EventId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
}

public class GetRecentPurchasesQueryHandler : IQueryHandler<GetRecentPurchasesQuery, PagedResult<RecentPurchaseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRecentPurchasesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<RecentPurchaseDto>> Handle(GetRecentPurchasesQuery query, CancellationToken cancellationToken)
    {
        // Verify the event exists
        var eventExists = await _context.Events
            .IgnoreQueryFilters()
            .AnyAsync(e => e.Id == query.EventId, cancellationToken);

        if (!eventExists)
            throw new KeyNotFoundException($"Event with ID {query.EventId} was not found.");

        // Build base query for order items related to this event
        var ordersQuery = _context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Ticket)
                .ThenInclude(t => t.EventDate)
                    .ThenInclude(ed => ed.Event)
            .IgnoreQueryFilters()
            .Where(oi => oi.Ticket.EventDate.EventId == query.EventId);

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchTerm = query.Search.Trim().ToLower();
            ordersQuery = ordersQuery.Where(oi =>
                oi.Order.BuyerEmail.ToLower().Contains(searchTerm));
        }

        // Get total count for pagination
        var totalCount = await ordersQuery.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var orderItems = await ordersQuery
            .OrderByDescending(oi => oi.Order.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var recentPurchases = orderItems.Select(oi => new RecentPurchaseDto
        {
            Id = oi.Id,
            TicketName = oi.Ticket.Name,
            Quantity = oi.Quantity,
            TotalPrice = oi.Price.Amount,
            Currency = oi.Price.Currency,
            PurchaseDate = oi.Order.CreatedAt,
            BuyerEmail = oi.Order.BuyerEmail,
            BuyerName = oi.Order.BuyerName,
            BuyerPhone = oi.Order.BuyerPhone,
            Tickets = new List<PurchasedTicketDetailDto>
            {
                new PurchasedTicketDetailDto
                {
                    Name = oi.Ticket.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price.Amount,
                    Currency = oi.Price.Currency,
                    Seats = oi.SeatsOrdered?.Select(sa => new SeatInfoDto
                    {
                        RowName = sa.RowName,
                        SeatNumber = sa.SeatNumber
                    })
                }
            }
        }).ToList();

        return new PagedResult<RecentPurchaseDto>(
            recentPurchases,
            totalCount,
            query.PageNumber,
            query.PageSize);
    }
}