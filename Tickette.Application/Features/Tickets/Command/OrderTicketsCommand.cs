using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Tickets.Common;
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
}

public class OrderTicketsCommandHandler : ICommandHandler<OrderTicketsCommand, ResponseTicketOrderedDto>
{
    private readonly IApplicationDbContext _context;

    public OrderTicketsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResponseTicketOrderedDto> Handle(OrderTicketsCommand command, CancellationToken cancellation)
    {
        // Check if the event exists
        var eventEntity = await _context.Events
            .SingleOrDefaultAsync(e => e.Id == command.EventId, cancellation);

        if (eventEntity == null)
        {
            throw new ArgumentException("The event does not exist.", nameof(command.EventId));
        }

        // Create an order record
        var order = new Order(command.EventId, command.BuyerId, command.BuyerEmail,
            command.BuyerName, command.BuyerPhone);

        // Fetch valid ticket IDs for the event
        var validTickets = await _context.Tickets
            .Where(t => t.EventId == command.EventId)
            .ToListAsync(cancellation);

        // Validate and calculate price
        decimal totalPrice = 0;
        foreach (var item in command.OrderItems)
        {
            var ticket = validTickets.SingleOrDefault(t => t.Id == item.TicketId);

            if (ticket == null)
            {
                throw new ArgumentException($"Invalid TicketId: {item.TicketId}. The ticket does not exist for the specified event.");
            }

            totalPrice += ticket.Price * item.Quantity;
        }

        order.SetFinalPrice(totalPrice);
        order.CountTotalQuantity(command.OrderItems.Sum(item => item.Quantity));

        await _context.Orders.AddAsync(order, cancellation);



        // Add order items
        foreach (var item in command.OrderItems)
        {
            // Find the ticket details
            var ticket = validTickets.SingleOrDefault(t => t.Id == item.TicketId);

            // Validate the ticket
            if (ticket == null)
            {
                throw new ArgumentException($"Invalid TicketId: {item.TicketId}. The ticket does not exist for the specified event.");
            }

            // Create the OrderItem from the DTO
            var orderTicketOrderItem = item.ToCreateTicketOrderItemDto();
            orderTicketOrderItem.SetTicketOrderId(order.Id);

            if (item.Quantity <= 0)
            {
                throw new ArgumentException($"Invalid quantity for TicketId: {item.TicketId}. Quantity must be greater than 0.");
            }

            // Add the OrderItem to the database
            await _context.OrderItems.AddAsync(orderTicketOrderItem, cancellation);
        }


        // Save changes to the database
        await _context.SaveChangesAsync(cancellation);

        // Return response
        return new ResponseTicketOrderedDto(command.EventId);
    }
}
