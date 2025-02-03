using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Orders.Command.CreateOrder;

public record CreateOrderCommand
{
    public required Guid UserId { get; init; }
    public required Guid EventId { get; init; }
    public required ICollection<TicketReservation> Tickets { get; set; }

    public string BuyerEmail { get; init; }
    public string BuyerName { get; init; }
    public string BuyerPhone { get; init; }
    public string? CouponCode { get; init; }
}

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IRedisService _redisService;

    public CreateOrderCommandHandler(IApplicationDbContext context, IRedisService redisService)
    {
        _context = context;
        _redisService = redisService;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand query, CancellationToken cancellation)
    {
        // Create an order
        var order = Order.CreateOrder(query.EventId, query.UserId, query.BuyerEmail, query.BuyerName, query.BuyerPhone);

        // Add order items
        foreach (var ticket in query.Tickets)
        {
            var ticketPrice = await _context.Tickets
                .Where(t => t.Id == ticket.Id)
                .Select(t => t.Price) // Nullable to detect missing records
                .SingleOrDefaultAsync(cancellation);

            var orderItem = OrderItem.Create(ticket.Id, ticketPrice, ticket.Quantity, null);

            order.AddOrderItem(orderItem.TicketId, orderItem.Price, orderItem.Quantity, null);
        }

        // If Coupon
        if (!string.IsNullOrWhiteSpace(query.CouponCode))
        {
            var coupon = await _context.Coupons
                .SingleOrDefaultAsync(c => c.Code == query.CouponCode, cancellation);
            if (coupon is not null)
            {
                order.ApplyCoupon(coupon);
            }
        }

        // Save order
        await _context.Orders.AddAsync(order, cancellation);

        await _context.SaveChangesAsync(cancellation);

        var response = new CreateOrderResponse
        {
            OrderId = order.Id,
        };

        foreach (var ticket in query.Tickets)
        {
            string reservationKey = $"reservation:{ticket.Id}:{query.UserId}";
            var exists = await _redisService.KeyExistsAsync(reservationKey);

            // No reservation found
            if (!exists)
            {
                // Increase the tickets quantity back
                string inventoryKey = $"ticket:{ticket.Id}:remaining_tickets";
                await _redisService.IncrementAsync(inventoryKey, ticket.Quantity);

                continue;
            }

            // Remove the reservation from Redis
            await _redisService.DeleteKeyAsync(reservationKey);

        }

        return response;
    }
}