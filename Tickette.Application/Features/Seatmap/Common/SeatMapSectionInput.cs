namespace Tickette.Application.Features.Seatmap.Common;

public record SeatMapSectionInput
{
    public string Height { get; init; }

    public string Width { get; init; }

    public double X { get; init; }

    public double Y { get; init; }

    public string? Number { get; init; }

    public string? RowName { get; init; }

    public string? Name { get; init; }

    public string Fill { get; init; }

    public string Quantity { get; init; }

    public Guid TicketId { get; init; }
}