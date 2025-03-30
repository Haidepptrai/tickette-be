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
    public required Guid UserId { get; set; }
}

public class ReviewOrdersQueryHandler : IQueryHandler<ReviewOrdersQuery, ResponseDto<List<OrderedTicketGroupListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeService _qrCodeService;

    public ReviewOrdersQueryHandler(IApplicationDbContext context, IQrCodeService qrCodeService)
    {
        _context = context;
        _qrCodeService = qrCodeService;
    }

    public async Task<ResponseDto<List<OrderedTicketGroupListDto>>> Handle(ReviewOrdersQuery query, CancellationToken cancellation)
    {
        try
        {
            var result = await _context.Orders
                .Where(order => order.UserOrderedId == query.UserId)
                .Include(order => order.Items)
                .ThenInclude(orderItem => orderItem.Ticket)
                .ThenInclude(ticket => ticket.EventDate) // Each ticket has a specific event date
                .ThenInclude(eventDate => eventDate.Event)
                .ToListAsync(cancellation);

            var groupedOrders = result
                .Select(order => new OrderedTicketGroupListDto()
                {
                    Id = order.Id,
                    EventName = order.Event.Name,
                    EventBanner = order.Event.Banner,
                    Address = string.Join(" at ", new[]
                    {
                        order.Event.LocationName, // Place name first
                        string.Join(", ", new[]
                        {
                            order.Event.StreetAddress,
                            order.Event.Ward,
                            order.Event.District,
                            order.Event.City
                        }.Where(s => !string.IsNullOrWhiteSpace(s)))
                    }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    OrderItems = order.Items
                        .Select(orderItem => new OrderItemListDto()
                        {
                            Id = orderItem.Id,
                            StartDate = orderItem.Ticket.EventDate.StartDate, // Assign event date from ticket
                            EndDate = orderItem.Ticket.EventDate.EndDate,
                            Tickets = new OrderedTicketDto()
                            {
                                Id = orderItem.Ticket.Id,
                                Name = orderItem.Ticket.Name,
                                Image = orderItem.Ticket.Image,
                                Description = orderItem.Ticket.Description,
                                Quantity = orderItem.Quantity,
                                TotalPrice = orderItem.Quantity * orderItem.Ticket.Price.Amount,
                                Currency = orderItem.Ticket.Price.Currency,
                                QrCode = GenerateQrCode(orderItem, order.UserOrderedId)
                            }
                        })
                        .ToList()
                })
                .ToList();


            return ResponseHandler.SuccessResponse(groupedOrders);
        }
        catch
        {
            throw new Exception("An error occurred while fetching the orders.");
        }
    }

    private string GenerateQrCode(OrderItem orderItem, Guid userId)
    {
        var qrCodeData = new OrderItemQrCodeDto
        {
            BuyerEmail = orderItem.Order.BuyerEmail,
            BuyerName = orderItem.Order.BuyerName,
            BuyerPhone = orderItem.Order.BuyerPhone,
            OrderId = orderItem.OrderId,
            OrderItemId = orderItem.Id
        };

        return _qrCodeService.GenerateQrCode(qrCodeData);
    }
}