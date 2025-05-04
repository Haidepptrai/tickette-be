using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Exceptions;
using Tickette.Application.Features.Orders.Command.CreateOrder;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Messaging.Feature;

public class ConfirmTicketReservationConsumer : IConsumer<CreateOrderCommand>
{
    private readonly IReservationService _reservationService;
    private readonly IApplicationDbContext _context;

    public ConfirmTicketReservationConsumer(IReservationService reservationService, IApplicationDbContext context)
    {
        _reservationService = reservationService;
        _context = context;
    }

    public async Task Consume(ConsumeContext<CreateOrderCommand> context)
    {
        try
        {
            var request = context.Message;

            foreach (var ticket in request.Tickets)
            {
                // 1. Remove from Redis
                await _reservationService.FinalizeSeatReservationAsync(request.UserId, ticket);

            }

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