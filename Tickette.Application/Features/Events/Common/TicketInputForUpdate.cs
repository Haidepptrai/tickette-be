using Tickette.Application.Common.Interfaces;

namespace Tickette.Application.Features.Events.Common;

public record TicketInputForUpdate
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; }

    public int TotalTickets { get; set; }

    public int MinPerOrder { get; set; }

    public int MaxPerOrder { get; set; }

    public DateTime SaleStartTime { get; set; }

    public DateTime SaleEndTime { get; set; }

    public string Description { get; set; }

    public IFileUpload? TicketImage { get; set; }
}