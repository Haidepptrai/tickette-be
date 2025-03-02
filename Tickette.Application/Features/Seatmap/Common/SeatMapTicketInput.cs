namespace Tickette.Application.Features.Seatmap.Common;

public record SeatMapTicketInput
{
    public Guid Id { get; init; }

    public IEnumerable<SeatMapSeatInput> Seats { get; init; }
}