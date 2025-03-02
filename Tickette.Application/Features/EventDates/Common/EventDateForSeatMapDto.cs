namespace Tickette.Application.Features.EventDates.Common;

public record EventDateForSeatMapDto
{
    public Guid Id { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public IEnumerable<TicketForSeatMapDto> Tickets { get; init; }
}