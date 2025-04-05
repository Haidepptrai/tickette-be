using Tickette.Application.Features.Orders.Common;
using Tickette.Application.Features.QRCode.Common;
using Tickette.Application.Features.QRCode.Queries.ValidateQrCode;

namespace Tickette.Application.Common.Interfaces;

public interface IQrCodeService
{
    TicketQrCode GenerateQrCode(OrderItemQrCodeDto order, int pixelSize = 20);
    bool VerifyQrCodeSignature(string data, string providedSignature);
    string SerializeData(ValidateQrCodeQuery request);
}