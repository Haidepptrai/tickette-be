namespace Tickette.Application.Features.QRCode.Common;

public record QrCodeDto
{
    public required byte[] QrCodeImage { get; set; }
}