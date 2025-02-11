namespace Tickette.Application.Features.QRCode.Common;

public record OrderItemQrCodeDto
{
    public string BuyerEmail { get; init; }

    public string BuyerName { get; init; }

    public string BuyerPhone { get; init; }

    public Guid OrderId { get; init; }

    public Guid OrderItemId { get; init; }

    //public ICollection<EventSeat>? SeatsOrdered { get; init; }
}