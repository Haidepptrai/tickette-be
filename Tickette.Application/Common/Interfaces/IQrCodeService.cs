using Tickette.Application.Features.QRCode.Common;

namespace Tickette.Application.Common.Interfaces;

public interface IQrCodeService
{
    byte[] GenerateQrCode(OrderItemQrCodeDto order, int pixelSize = 20);
    (OrderItemQrCodeDto?, bool) DecodeQrCode(byte[] qrCodeBytes);
}