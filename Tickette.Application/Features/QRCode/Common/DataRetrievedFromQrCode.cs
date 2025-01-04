namespace Tickette.Application.Features.QRCode.Common;

public class DataRetrievedFromQrCode
{
    public bool IsValid { get; set; }
    public OrderDetailFromQrCodeDto OrderDetail { get; set; }
}