namespace Tickette.Application.Features.Events.Common;

public record EventDateInputForUpdate
{
    public Guid Id { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public required List<TicketInputForUpdate> Tickets { get; set; }
}