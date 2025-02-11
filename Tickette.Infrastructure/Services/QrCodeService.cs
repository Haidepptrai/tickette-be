using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using QRCoder;
using System.Security.Cryptography;
using Tickette.Application.Common;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.QRCode.Common;
using Tickette.Application.Features.QRCode.Queries.ValidateQrCode;

namespace Tickette.Infrastructure.Services;

public class QrCodeService : IQrCodeService
{
    private readonly string _secretKey;

    public QrCodeService(IConfiguration configuration)
    {
        _secretKey = configuration["QrCodeSecretKey"] ?? throw new KeyNotFoundException("Missing Secret Key For QR Code Generator");
    }

    public string GenerateQrCode(OrderItemQrCodeDto order, int pixelSize = 20)
    {
        // 1. Serialize the OrderItemQrCodeDto to JSON
        var jsonData = JsonConvert.SerializeObject(order);

        // 2. Generate a HMAC signature using the secret key
        var signature = GenerateSignature(jsonData);

        // 3. Create a payload with the order data and the signature
        var payload = new QrCodePayload(jsonData, signature);

        // 4. Serialize the payload to JSON
        var payloadJson = JsonConvert.SerializeObject(payload);

        // 5. Generate the QR code using PngByteQRCode
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(payloadJson, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);

        // 6. Get the QR code as a PNG byte array
        byte[] qrCodeBytes = qrCode.GetGraphic(10);

        // 7. Convert bytes to Base64 string
        return Convert.ToBase64String(qrCodeBytes);
    }

    public bool VerifyQrCodeSignature(string data, string providedSignature)
    {
        var expectedSignature = GenerateSignature(data);
        return expectedSignature == providedSignature;
    }

    public string SerializeData(ValidateQrCodeQuery request)
    {
        return JsonConvert.SerializeObject(request);
    }

    private string GenerateSignature(string data)
    {
        using var hmac = new HMACSHA256(System.Text.Encoding.UTF8.GetBytes(_secretKey));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}
