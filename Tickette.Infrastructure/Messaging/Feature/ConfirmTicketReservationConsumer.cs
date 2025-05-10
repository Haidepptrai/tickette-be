using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Tickette.Application.Common.Constants;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Common.Interfaces.Stripe;
using Tickette.Application.Common.Models.Email;
using Tickette.Application.Exceptions;
using Tickette.Application.Factories;
using Tickette.Application.Features.Orders.Command.CreateOrder;
using Tickette.Domain.Common.Exceptions;
using Tickette.Domain.Entities;
using Tickette.Domain.ValueObjects;

namespace Tickette.Infrastructure.Messaging.Feature;

public class ConfirmTicketReservationConsumer : IConsumer<CreateOrderCommand>
{
    private readonly IReservationService _reservationService;
    private readonly IApplicationDbContext _context;
    private readonly IMessageRequestClient _messageRequestClient;
    private readonly IPaymentService _paymentService;

    public ConfirmTicketReservationConsumer(IReservationService reservationService, IApplicationDbContext context, IMessageRequestClient messageRequestClient, IPaymentService paymentService)
    {
        _reservationService = reservationService;
        _context = context;
        _messageRequestClient = messageRequestClient;
        _paymentService = paymentService;
    }

    public async Task Consume(ConsumeContext<CreateOrderCommand> context)
    {
        try
        {
            var request = context.Message;

            var isPaymentValid = await _paymentService.ValidatePayment(request.PaymentIntentId);

            if (!isPaymentValid)
            {
                throw new TicketReservationException("Not valid payment"); // This could have a log to check out
            }

            foreach (var ticket in request.Tickets)
            {
                // Remove from Redis
                await _reservationService.FinalizeSeatReservationAsync(request.UserId, ticket);
            }

            // Create an order
            var order = Order.CreateOrder(request.EventId, request.UserId, request.BuyerEmail, request.BuyerName, request.BuyerPhone);

            string ticketDetailsHtml = string.Empty;

            // Add tickets to order
            foreach (var ticket in request.Tickets)
            {
                var ticketInfo = await _context.Tickets
                    .Where(t => t.Id == ticket.Id)
                    .SingleOrDefaultAsync();

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
            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var coupon = await _context.Coupons
                    .SingleOrDefaultAsync(c => c.Code == request.CouponCode);
                if (coupon is not null)
                {
                    order.ApplyCoupon(coupon);
                }
            }

            // Save order
            await _context.Orders.AddAsync(order);

            await _context.SaveChangesAsync();

            // Send email
            var firstTicket = await _context.Tickets
                .Where(t => request.Tickets.Select(qt => qt.Id).Contains(t.Id))
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
                .FirstOrDefaultAsync();

            if (firstTicket is null)
            {
                throw new Exception("Event details not found for the selected tickets.");
            }

            string eventStartDate = firstTicket.StartDate.ToString("MMMM dd, yyyy hh:mm tt"); // Example: June 15, 2025 07:00 PM
            string eventEndDate = firstTicket.EndDate.ToString("MMMM dd, yyyy hh:mm tt");

            // Format Location
            string fullLocation = $"{firstTicket.LocationName}, {firstTicket.StreetAddress}, {firstTicket.Ward}, {firstTicket.District}, {firstTicket.City}";


            var emailMessage = new OrderPaymentSuccessEmail(
                request.BuyerName,
                request.BuyerEmail,
                request.BuyerPhone,
                firstTicket.Name,       // Event Name
                eventStartDate,         // Start Date & Time
                eventEndDate,           // End Date & Time
            fullLocation,           // Formatted Location
                ticketDetailsHtml,
                $"/tickets/download/{order.Id}"
            );

            var emailWrapper = EmailWrapperFactory.Create(EmailServiceKeys.EmailConfirmCreateOrder, emailMessage);

            await _messageRequestClient.FireAndForgetAsync(emailWrapper);

            // 2. Update seat map JSON in db if have
            if (request.HasSeatMap)
            {
                var eventDate = await _context.EventDates
                    .SingleOrDefaultAsync(e => e.Id == request.EventDateId);

                if (eventDate is null)
                    throw new NotFoundException("EventDate", request.EventId);

                var selectedSeats = request.Tickets
                    .Where(t => t.SeatsChosen != null)
                    .SelectMany(t => t.SeatsChosen!)
                    .ToList();

                var cloned = JsonSerializer.Deserialize<EventSeatMap>(
                    JsonSerializer.Serialize(eventDate.SeatMap))!;

                cloned.MarkSeatsAsOrdered(selectedSeats);
                eventDate.SeatMap = cloned;

                await _context.SaveChangesAsync();


                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Fatal] Error in TicketReservationCancelled consumer: {ex.Message}");
            // Log and exit gracefully — don't rethrow
        }
    }
}