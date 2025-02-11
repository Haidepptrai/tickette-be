namespace Tickette.Application.Common;

public record QrCodePayload
{
    public string Data { get; }
    public string Signature { get; }

    public QrCodePayload(string data, string signature)
    {
        Data = data;
        Signature = signature;
    }
}