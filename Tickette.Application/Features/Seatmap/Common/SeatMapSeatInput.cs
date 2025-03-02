namespace Tickette.Application.Features.Seatmap.Common;

public record SeatMapSeatInput
{
    public string Number { get; init; }

    public string Name { get; init; }

    public double X { get; init; }

    public double Y { get; init; }
}