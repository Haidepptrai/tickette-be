using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Stripe;
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
    private readonly IPaymentService _paymentService;

    public CreateOrderCommandHandler(IApplicationDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand query, CancellationToken cancellation)
    {
        // Create an order
        var order = Order.CreateOrder(query.EventId, query.UserId, query.BuyerEmail, query.BuyerName, query.BuyerPhone);

        // Add order items
        foreach (var ticket in query.Tickets)
        {
            var ticketPrice = await _context.Tickets
                .Where(t => t.Id == ticket.TicketId)
                .Select(t => (long?)t.Price) // Nullable to detect missing records
                .SingleOrDefaultAsync(cancellation);

            if (ticketPrice == null)
            {
                throw new ArgumentException($"Invalid Ticket ID: {ticket.TicketId}");
            }

            var orderItem = OrderItem.Create(ticket.TicketId, ticketPrice.Value, ticket.Quantity, null);

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

        // Create payment intent
        var payment = Payment.Create(order.TotalPrice);
        var paymentIntentResult = await _paymentService.CreatePaymentIntentAsync(payment);

        // Save order
        await _context.Orders.AddAsync(order, cancellation);

        await _context.SaveChangesAsync(cancellation);

        var response = new CreateOrderResponse
        {
            OrderId = order.Id,
            PaymentIntentId = paymentIntentResult.PaymentIntentId,
            ClientSecret = paymentIntentResult.ClientSecret
        };

        return response;
    }
}