using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Tickets.Common;
using Tickette.Application.Helpers;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Tickets.Command;

public record OrderTicketsCommand
{
    public Guid EventId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerEmail { get; init; }
    public string BuyerName { get; init; }
    public string BuyerPhone { get; init; }
    public required List<TicketOrderItemDto> OrderItems { get; init; }
    public string? CouponCode { get; init; }
}

public class OrderTicketsCommandHandler : ICommandHandler<OrderTicketsCommand, ResponseDto<Guid>>
{
    private readonly IApplicationDbContext _context;

    public OrderTicketsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResponseDto<Guid>> Handle(OrderTicketsCommand command, CancellationToken cancellation)
    {
        try
        {
            // Check if the event exists
            var eventEntity = await _context.Events
                .SingleOrDefaultAsync(e => e.Id == command.EventId, cancellation);

            if (eventEntity == null)
            {
                throw new ArgumentException("The event does not exist.", nameof(command.EventId));
            }

            // Fetch valid ticket IDs and prices for the event
            var validTickets = await _context.Tickets
                .Where(t => t.EventId == command.EventId)
                .ToDictionaryAsync(t => t.Id, t => t.Price, cancellation);

            // Create an Order
            var order = Order.CreateOrder(command.EventId, command.BuyerId, command.BuyerEmail,
                command.BuyerName, command.BuyerPhone);

            // Add order items
            foreach (var item in command.OrderItems)
            {
                if (!validTickets.TryGetValue(item.TicketId, out var ticketPrice))
                {
                    throw new ArgumentException(
                        $"Invalid TicketId: {item.TicketId}. The ticket does not exist for the specified event.");
                }

                // Fetch the selected seats from the database
                List<EventSeat>? selectedSeats = null;

                if (item.SelectedSeats != null && item.SelectedSeats.Any())
                {
                    selectedSeats = await _context.EventSeats
                        .Where(seat => item.SelectedSeats.Contains(seat.Id) && seat.IsAvailable)
                        .ToListAsync(cancellation);

                    if (selectedSeats.Count != item.SelectedSeats.Count)
                    {
                        throw new InvalidOperationException("One or more selected seats are not available.");
                    }

                    // Mark the seats as reserved (IsAvailable = false)
                    foreach (var seat in selectedSeats)
                    {
                        seat.SetAsReserved();
                    }

                }

                // Add the order item with the selected seats
                order.AddOrderItem(item.TicketId, ticketPrice, item.Quantity, selectedSeats);
            }


            if (command.CouponCode is not null)
            {
                // Apply coupon code
                var coupon = await _context.Coupons
                    .SingleOrDefaultAsync(c => c.Code == command.CouponCode.ToUpper(), cancellation);

                if (coupon is null)
                {
                    throw new ArgumentException("Invalid coupon code.", nameof(command.CouponCode));
                }

                order.ApplyCoupon(coupon);
            }

            // Save to the database
            await _context.Orders.AddAsync(order, cancellation);
            await _context.SaveChangesAsync(cancellation);

            // Return response
            return ResponseHandler.SuccessResponse(command.EventId);

        }
        catch (Exception ex)
        {
            return ResponseHandler.ErrorResponse(Guid.Empty, ex.Message);
        }
    }
}
