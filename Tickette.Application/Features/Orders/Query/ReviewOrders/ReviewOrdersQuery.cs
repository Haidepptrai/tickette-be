using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Orders.Common;
using Tickette.Application.Features.QRCode.Common;
using Tickette.Application.Wrappers;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Orders.Query.ReviewOrders;

public record ReviewOrdersQuery
{
    public Guid UserId { get; set; }

    public required int PageNumber { get; init; }

    public required int PageSize { get; init; }

    public string? TicketType { get; init; } = null; // "Active", "Recent", "Used"

    public string? Search { get; init; }
}

public class ReviewOrdersQueryHandler : IQueryHandler<ReviewOrdersQuery, PagedResult<OrderedTicketGroupListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeService _qrCodeService;

    public ReviewOrdersQueryHandler(IApplicationDbContext context, IQrCodeService qrCodeService)
    {
        _context = context;
        _qrCodeService = qrCodeService;
    }

    public async Task<PagedResult<OrderedTicketGroupListDto>> Handle(ReviewOrdersQuery query, CancellationToken cancellation)
    {
        var now = DateTime.UtcNow;

        // Normalize TicketType
        var ticketType = query.TicketType?.Trim().ToLowerInvariant();

        IQueryable<Order> baseQuery = _context.Orders
            .Where(o => o.UserOrderedId == query.UserId)
            .Include(o => o.Items)
                .ThenInclude(i => i.Ticket)
                    .ThenInclude(t => t.EventDate)
                        .ThenInclude(ed => ed.Event);

        // Apply search filtering if Search is provided
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLower();

            baseQuery = baseQuery.Where(o =>
                o.BuyerName.ToLower().Contains(keyword) ||
                o.BuyerEmail.ToLower().Contains(keyword) ||
                o.Items.Any(i =>
                    i.Ticket.Name.ToLower().Contains(keyword) ||
                    i.Ticket.EventDate.Event.Name.ToLower().Contains(keyword)
                ));
        }

        // Apply TicketType-specific filtering
        if (ticketType is not null)
        {
            switch (ticketType)
            {
                case "active":
                    baseQuery = baseQuery.Where(o =>
                        o.Items.Any(i => !i.IsScanned && i.Ticket.EventDate.EndDate >= now));
                    break;

                case "used":
                    baseQuery = baseQuery.Where(o =>
                        o.Items.Any(i => i.Ticket.EventDate.EndDate < now));
                    break;

                case "recent":
                    baseQuery = baseQuery.OrderByDescending(o => o.CreatedAt);
                    break;

                default:
                    return new PagedResult<OrderedTicketGroupListDto>(
                        items: [],
                        totalCount: 0,
                        pageNumber: query.PageNumber,
                        pageSize: query.PageSize
                    );
            }
        }

        // Get total count before pagination
        var totalCount = await baseQuery.CountAsync(cancellation);

        // Apply pagination
        var pagedOrders = await baseQuery
            .OrderByDescending(o => o.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellation);

        // Transform result
        var grouped = pagedOrders.Select(order =>
        {
            var eventDate = order.Items.First().Ticket.EventDate;
            var eventInfo = eventDate.Event;

            var address = string.Join(" at ", new[]
            {
            eventInfo.LocationName,
            string.Join(", ", new[]
            {
                eventInfo.StreetAddress,
                eventInfo.Ward,
                eventInfo.District,
                eventInfo.City
            }.Where(s => !string.IsNullOrWhiteSpace(s)))
        }.Where(s => !string.IsNullOrWhiteSpace(s)));

            // Filter items based on TicketType again, but now in memory
            var filteredItems = ticketType switch
            {
                "active" => order.Items.Where(i => !i.IsScanned && i.Ticket.EventDate.EndDate >= now).ToList(),
                "used" => order.Items.Where(i => i.IsScanned || i.Ticket.EventDate.EndDate < now).ToList(),
                "recent" => order.Items.ToList(), // For recent orders, include all items without filtering
                _ => order.Items.ToList()
            };

            var orderItems = filteredItems.Select(orderItem => new OrderItemListDto
            {
                Id = orderItem.Id,
                StartDate = orderItem.Ticket.EventDate.StartDate,
                EndDate = orderItem.Ticket.EventDate.EndDate,
                BuyerName = order.BuyerName,
                BuyerEmail = order.BuyerEmail,
                BuyerPhone = order.BuyerPhone,
                Tickets = new OrderedTicketDto
                {
                    Id = orderItem.Ticket.Id,
                    Name = orderItem.Ticket.Name,
                    Image = orderItem.Ticket.Image,
                    Description = orderItem.Ticket.Description,
                    Quantity = orderItem.Quantity,
                    TotalPrice = orderItem.Quantity * orderItem.Ticket.Price.Amount,
                    Currency = orderItem.Ticket.Price.Currency,
                    QrCode = GenerateQrCode(orderItem)
                }
            }).ToList();

            return new OrderedTicketGroupListDto
            {
                Id = order.Id,
                EventName = eventInfo.Name,
                EventBanner = eventInfo.Banner,
                Address = address,
                OrderItems = orderItems
            };
        }).Where(g => g.OrderItems.Any()) // Exclude any orders that got filtered to empty
        .ToList();

        return new PagedResult<OrderedTicketGroupListDto>(
            items: grouped,
            totalCount: totalCount,
            pageNumber: query.PageNumber,
            pageSize: query.PageSize
        );
    }

    private TicketQrCode GenerateQrCode(OrderItem orderItem)
    {
        var qrCodeData = new OrderItemQrCodeDto
        {
            BuyerEmail = orderItem.Order.BuyerEmail,
            BuyerName = orderItem.Order.BuyerName,
            BuyerPhone = orderItem.Order.BuyerPhone,
            OrderId = orderItem.OrderId,
            OrderItemId = orderItem.Id,
            SeatsOrdered = orderItem.SeatsOrdered
        };

        return _qrCodeService.GenerateQrCode(qrCodeData);
    }
}