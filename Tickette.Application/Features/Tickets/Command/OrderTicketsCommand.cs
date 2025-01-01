using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Tickets.Common;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Tickets.Command;

public record OrderTicketsCommand
{
    public Guid TicketId { get; init; }
    public Guid EventId { get; init; }
    public Guid BuyerId { get; init; }
    public int Quantity { get; init; }
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

        // Check if the ticket exists and is available
        var ticketEntity = await _context.Tickets
            .SingleOrDefaultAsync(t => t.Id == command.TicketId && t.EventId == command.EventId, cancellation);

        if (ticketEntity == null)
        {
            throw new ArgumentException("The ticket does not exist for the specified event.", nameof(command.TicketId));
        }

        if (ticketEntity.RemainingTickets < command.Quantity)
        {
            throw new InvalidOperationException("Not enough tickets are available.");
        }

        // Reduce the available tickets
        ticketEntity.UpdateRemainingTickets(command.Quantity);

        // Create an order record
        var order = new TicketOrder(command.TicketId, command.EventId, command.BuyerId, command.Quantity, command.BuyerEmail,
            command.BuyerName, command.BuyerPhone);


        await _context.TicketOrders.AddAsync(order, cancellation);

        // Add order items
        foreach (var item in command.OrderItems)
        {
            var orderTicketOrderItem = item.ToCreateTicketOrderItemDto();
            orderTicketOrderItem.SetTicketOrderId(order.Id);
            await _context.TicketOrderItems.AddAsync(orderTicketOrderItem, cancellation);
        }

        // Save changes to the database
        await _context.SaveChangesAsync(cancellation);

        // Return response
        return new ResponseTicketOrderedDto(command.EventId);
    }
}
