using MassTransit;
using System.Collections.Concurrent;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Command.ReserveTicket;
using Tickette.Application.Wrappers;
using Tickette.Domain.Common.Exceptions;

namespace Tickette.Infrastructure.Messaging.Feature;

/// <summary>
/// This class is responsible for synchronizing the ticket reservation with database
/// after done in Redis.
/// </summary>
public class ReserveTickerConsumer : IConsumer<ReserveTicketCommand>
{
    private readonly IReservationService _reservationService;

    public ReserveTickerConsumer(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public async Task Consume(ConsumeContext<ReserveTicketCommand> context)
    {
        var request = context.Message;
        var errors = new ConcurrentBag<RedisReservationResult>();

        try
        {
            await _reservationService.ReserveTicketsAsync(request.UserId, request.Tickets);
        }
        catch (SeatOrderedException ex)
        {
            //await _reservationService.ReleaseReservationAsync(request.UserId, request.Tickets);
            errors.Add(RedisReservationResult.Fail(ex.Message, "SeatConflict"));
        }
        catch (TicketReservationException ex)
        {
            //await _reservationService.ReleaseReservationAsync(request.UserId, ticket);
            errors.Add(RedisReservationResult.Fail(ex.Message, "InventoryIssue"));
        }
        catch (Exception)
        {
            //await _reservationService.ReleaseReservationAsync(request.UserId, ticket);
            errors.Add(RedisReservationResult.Fail("Unexpected error", "UnhandledException"));
        }


        if (!errors.IsEmpty)
        {
            await context.RespondAsync(errors.First());
        }
        else
        {
            await context.RespondAsync(RedisReservationResult.Ok());
        }
    }

}