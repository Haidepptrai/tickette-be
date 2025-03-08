using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Domain.Entities;
using Tickette.Domain.Enums;
using Tickette.Infrastructure.Helpers;

namespace Tickette.Application.Features.Events.Commands.UpdateEventStatus;

public record UpdateEventStatusCommand(Guid EventId, ApprovalStatus Status);

public class UpdateEventStatusHandler : ICommandHandler<UpdateEventStatusCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IRedisService _redisService;

    public UpdateEventStatusHandler(IApplicationDbContext context, IRedisService redisService)
    {
        _context = context;
        _redisService = redisService;
    }

    public async Task<Guid> Handle(UpdateEventStatusCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Fetch the event with its dates and tickets
            var eventToUpdate = await _context.Events
                .Include(e => e.EventDates)
                    .ThenInclude(ed => ed.Tickets) // Include tickets for each event date
                .AsSplitQuery()
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(ev => ev.Id == command.EventId, cancellationToken);

            if (eventToUpdate == null)
                throw new KeyNotFoundException($"Event with ID {command.EventId} was not found.");

            if (eventToUpdate.Status == ApprovalStatus.Approved)
                throw new InvalidOperationException("Event status cannot be updated once it is approved.");

            // Update the event status
            eventToUpdate.Status = command.Status;

            // If the event is approved, initialize Redis with ticket quantities for each event date
            if (command.Status == ApprovalStatus.Approved)
            {
                var redisData = new Dictionary<string, string>();

                foreach (var eventDate in eventToUpdate.EventDates)
                {
                    foreach (var ticket in eventDate.Tickets)
                    {
                        string inventoryKey = RedisKeys.GetTicketQuantityKey(ticket.Id);

                        // Only add if it doesn't already exist in Redis
                        var existingValue = await _redisService.GetAsync(inventoryKey);
                        if (existingValue == null || existingValue == "0")
                        {
                            redisData[inventoryKey] = ticket.RemainingTickets.ToString();
                        }
                    }
                }


                if (redisData.Count > 0)
                {
                    await _redisService.SetBatchAsync(redisData);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Ensure all approved event tickets exist in Redis
            await VerifyAllTicketsExistInRedis(eventToUpdate);

            return command.EventId;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error updating event status: {ex.Message}", ex);
        }
    }

    private async Task VerifyAllTicketsExistInRedis(Event eventToUpdate)
    {
        foreach (var eventDate in eventToUpdate.EventDates)
        {
            foreach (var ticket in eventDate.Tickets)
            {
                string inventoryKey = RedisKeys.GetTicketQuantityKey(ticket.Id);

                var cachedValue = await _redisService.GetAsync(inventoryKey);
                if (cachedValue == null)
                {
                    // If missing, re-add it
                    await _redisService.SetAsync(inventoryKey, ticket.RemainingTickets.ToString(), 0);
                    Console.WriteLine($"Ticket {ticket.Id} added to Redis.");
                }
            }
        }
    }
}