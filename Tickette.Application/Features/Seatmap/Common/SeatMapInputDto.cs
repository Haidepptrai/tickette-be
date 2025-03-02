namespace Tickette.Application.Features.Seatmap.Common;

public record SeatMapInputDto
{
    public IEnumerable<SeatMapSectionInput> Sections { get; init; }

    public IEnumerable<SeatMapTicketInput> Tickets { get; init; }

    public IEnumerable<SeatMapSeatInput> UnassignedSeats { get; init; }
}