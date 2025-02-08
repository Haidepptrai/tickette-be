using Tickette.Domain.Entities;

namespace Tickette.Application.Features.QRCode.Common;

public record OrderItemQrCodeDto
{
    public string BuyerEmail { get; init; }

    public string BuyerName { get; init; }

    public string BuyerPhone { get; init; }

    public Guid OrderItemId { get; init; }

    public Guid EventId { get; init; }

    public string TicketName { get; init; }

    public DateTime TicketEventStartTime { get; init; }
    public DateTime TicketEventEndTime { get; init; }
    public ICollection<EventSeat>? SeatsOrdered { get; init; }
}