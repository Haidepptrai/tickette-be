namespace Tickette.Application.Features.Events.Common;

public class TicketDto
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public decimal Amount { get; init; }

    public string Currency { get; init; }

    public int TotalTickets { get; init; }

    public int MinPerOrder { get; init; }

    public int MaxPerOrder { get; init; }

    public DateTime SaleStartTime { get; init; }

    public DateTime SaleEndTime { get; init; }

    public string Description { get; init; }

    public string? TicketImage { get; init; }

    public bool IsFree { get; init; }
}