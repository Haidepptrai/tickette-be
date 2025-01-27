namespace Tickette.Application.Features.Events.Common;

public record EventDateInput
{
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public required List<TicketInput> Tickets { get; set; }

}