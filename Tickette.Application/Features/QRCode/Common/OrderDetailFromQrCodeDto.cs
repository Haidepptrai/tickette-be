namespace Tickette.Application.Features.QRCode.Common;

public class OrderDetailFromQrCodeDto
{
    public Guid EventId { get; init; }

    public Guid TicketId { get; init; }

    public ICollection<Guid>? SeatIds { get; init; }

    public OrderDetailFromQrCodeDto(Guid eventId, Guid ticketId, ICollection<Guid>? seatIds)
    {
        EventId = eventId;
        TicketId = ticketId;
        SeatIds = seatIds;
    }
}