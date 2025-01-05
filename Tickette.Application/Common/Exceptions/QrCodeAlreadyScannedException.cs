namespace Tickette.Application.Common.Exceptions;

public class QrCodeAlreadyScannedException : Exception
{
    public QrCodeAlreadyScannedException() : base("QR Code already scanned")
    {
    }
}