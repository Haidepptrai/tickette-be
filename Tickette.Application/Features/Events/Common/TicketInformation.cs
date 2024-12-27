using Tickette.Application.Common.Interfaces;

namespace Tickette.Application.Features.Events.Common;

public record TicketInformation
{
    public string Name { get; init; }

    public decimal Price { get; init; }

    public int TotalTickets { get; init; }

    public int MinTicketsPerOrder { get; init; }

    public int MaxTicketsPerOrder { get; init; }

    public DateTime SaleStartTime { get; init; }

    public DateTime SaleEndTime { get; init; }

    public DateTime EventStartTime { get; init; }

    public DateTime EventEndTime { get; init; }

    public string Description { get; init; }

    public IFileUpload? TicketImage { get; init; }
}
