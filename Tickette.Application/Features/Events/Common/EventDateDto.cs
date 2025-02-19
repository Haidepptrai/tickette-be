namespace Tickette.Application.Features.Events.Common;

public record EventDateDto
{
    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public IEnumerable<TicketDto> Tickets { get; init; }
}