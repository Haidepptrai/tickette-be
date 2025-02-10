using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Email;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Common.Models;
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
    private readonly IEmailService _emailService;

    public CreateOrderCommandHandler(IApplicationDbContext context, IRedisService redisService, IEmailService emailService)
    {
        _context = context;
        _redisService = redisService;
        _emailService = emailService;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand query, CancellationToken cancellation)
    {
        // Create an order
        var order = Order.CreateOrder(query.EventId, query.UserId, query.BuyerEmail, query.BuyerName, query.BuyerPhone);

        string ticketDetailsHtml = "<tr><th>Ticket Type</th><th>Quantity</th><th>Total Price</th></tr>";
        // Add order items
        foreach (var ticket in query.Tickets)
        {
            var ticketInfo = await _context.Tickets
                .Where(t => t.Id == ticket.Id)
                .SingleOrDefaultAsync(cancellation);

            if (ticketInfo is not null)
            {
                decimal totalPrice = ticketInfo.Price * ticket.Quantity;
                ticketDetailsHtml += $"<tr><td>{ticketInfo.Name}</td><td>{ticket.Quantity}</td><td>${totalPrice:F2}</td></tr>";

                var orderItem = OrderItem.Create(ticket.Id, ticketInfo.Price, ticket.Quantity, null);
                order.AddOrderItem(orderItem.TicketId, orderItem.Price, orderItem.Quantity, null);
            }
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

        // Send email
        var firstTicket = await _context.Tickets
            .Where(t => query.Tickets.Select(qt => qt.Id).Contains(t.Id))
            .Select(t => new
            {
                t.EventDate.Event.Name,             // Event Name
                t.EventDate.StartDate,              // Start Date & Time
                t.EventDate.EndDate,                // End Date & Time
                t.EventDate.Event.LocationName,     // Venue Name
                t.EventDate.Event.City,             // City
                t.EventDate.Event.District,         // District
                t.EventDate.Event.Ward,             // Ward
                t.EventDate.Event.StreetAddress     // Street Address
            })
            .FirstOrDefaultAsync(cancellation);

        if (firstTicket is null)
        {
            throw new Exception("Event details not found for the selected tickets.");
        }

        string eventStartDate = firstTicket.StartDate.ToString("MMMM dd, yyyy hh:mm tt"); // Example: June 15, 2025 07:00 PM
        string eventEndDate = firstTicket.EndDate.ToString("MMMM dd, yyyy hh:mm tt");

        // Format Location
        string fullLocation = $"{firstTicket.LocationName}, {firstTicket.StreetAddress}, {firstTicket.Ward}, {firstTicket.District}, {firstTicket.City}";

        var emailModel = new OrderPaymentSuccessEmail(
            query.BuyerName,
            query.BuyerEmail,
            query.BuyerPhone,
            firstTicket.Name,       // Event Name
            eventStartDate,         // Start Date & Time
            eventEndDate,           // End Date & Time
            fullLocation,           // Formatted Location
            ticketDetailsHtml,
            $"/tickets/download/{order.Id}"
        );

        await _emailService.SendEmailAsync(query.BuyerEmail, "Order Process Successfully", "ticket_order_confirmation", emailModel);


        return response;
    }
}