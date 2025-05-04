using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Common.Models.Email;
using Tickette.Application.Factories;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Entities;
using Tickette.Domain.ValueObjects;

namespace Tickette.Application.Features.Orders.Command.CreateOrder;

public record CreateOrderCommand
{
    public Guid UserId { get; set; }
    public required Guid EventId { get; init; }
    public required Guid EventDateId { get; init; }
    public required ICollection<TicketReservation> Tickets { get; set; }

    public string BuyerEmail { get; init; }
    public string BuyerName { get; init; }
    public string BuyerPhone { get; init; }
    public string? CouponCode { get; init; }

    public bool HasSeatMap { get; set; } = false;
}

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IReservationService _reservationService;
    private readonly IMessageRequestClient _messageRequestClient;

    public CreateOrderCommandHandler(
        IApplicationDbContext context,
        IReservationService reservationService,
        IMessageRequestClient messageRequestClient)
    {
        _context = context;
        _reservationService = reservationService;
        _messageRequestClient = messageRequestClient;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand query, CancellationToken cancellation)
    {
        // Create an order
        var order = Order.CreateOrder(query.EventId, query.UserId, query.BuyerEmail, query.BuyerName, query.BuyerPhone);

        string ticketDetailsHtml = string.Empty;

        // Validate the ticket reservations
        foreach (var ticket in query.Tickets)
        {
            await _reservationService.ValidateReservationAsync(query.UserId, ticket);
        }

        // Add tickets to order
        foreach (var ticket in query.Tickets)
        {
            var ticketInfo = await _context.Tickets
                .Where(t => t.Id == ticket.Id)
                .SingleOrDefaultAsync(cancellation);

            if (ticketInfo is not null)
            {
                Price totalPrice = ticketInfo.Price * ticket.Quantity;
                ticketDetailsHtml += $"<tr><td>{ticketInfo.Name}</td><td>{ticket.Quantity}</td><td>{totalPrice.Format()}</td></tr>";

                var orderItem = OrderItem.Create(ticket.Id, ticketInfo.Price, ticket.Quantity, ticket.SectionName, ticket.SeatsChosen?.ToList());
                order.AddOrderItem(orderItem);
            }

            // Reduce the ticket quantity in the database
            if (ticketInfo is not null)
            {
                ticketInfo.ReduceTickets(ticket.Quantity);
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
            OrderId = order.Id
        };

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


        var emailMessage = new OrderPaymentSuccessEmail(
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

        var emailWrapper = EmailWrapperFactory.Create(EmailServiceKeys.EmailConfirmCreateOrder, emailMessage);

        await _messageRequestClient.FireAndForgetAsync(emailWrapper, cancellation);

        // Publish message to RabbitMQ to set reservation to confirm
        await _messageRequestClient.FireAndForgetAsync(query, cancellation);

        return response;
    }
}