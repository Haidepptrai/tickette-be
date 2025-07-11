﻿using Tickette.Domain.Entities;

namespace Tickette.Application.Features.QRCode.Common;

public record OrderItemQrCodeDto
{
    public string BuyerEmail { get; init; }

    public string BuyerName { get; init; }

    public string BuyerPhone { get; init; }

    public Guid OrderId { get; init; }

    public Guid OrderItemId { get; init; }

    public ICollection<SeatOrder>? SeatsOrdered { get; init; }
}