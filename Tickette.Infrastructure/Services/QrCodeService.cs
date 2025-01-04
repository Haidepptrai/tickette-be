using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using QRCoder;
using SkiaSharp;
using System.Security.Cryptography;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.QRCode.Common;
using ZXing.SkiaSharp;

namespace Tickette.Infrastructure.Services;

public class QrCodeService : IQrCodeService
{
    private readonly string _secretKey;

    public QrCodeService(IConfiguration configuration)
    {
        _secretKey = configuration["QrCodeSecretKey"] ?? throw new KeyNotFoundException("Missing Secret Key For QR Code Generator");
    }

    public byte[] GenerateQrCode(OrderItemQrCodeDto order, int pixelSize = 20)
    {
        // 1. Serialize the OrderItemQrCodeDto to JSON
        var jsonData = JsonConvert.SerializeObject(order);

        // 2. Generate a HMAC signature using the secret key
        var signature = GenerateSignature(jsonData);

        // 3. Create a payload with the order data and the signature
        var payload = new
        {
            Data = jsonData,
            Signature = signature
        };

        // 4. Serialize the payload to JSON
        var payloadJson = JsonConvert.SerializeObject(payload);

        // 5. Generate the QR code using PngByteQRCode
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(payloadJson, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);

        // 6. Return the QR code as a PNG byte array
        return qrCode.GetGraphic(pixelSize);
    }

    public (OrderItemQrCodeDto?, bool) DecodeQrCode(byte[] qrCodeBytes)
    {
        try
        {
            // 1. Load the byte array into a SkiaSharp bitmap
            using var skBitmap = SKBitmap.Decode(qrCodeBytes);

            // 2. Use ZXing with SkiaSharp binding to decode the QR code
            var reader = new BarcodeReader
            {
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<ZXing.BarcodeFormat> { ZXing.BarcodeFormat.QR_CODE }
                }
            };

            var result = reader.Decode(skBitmap);

            // 3. Ensure the QR code was decoded successfully
            if (result == null || string.IsNullOrWhiteSpace(result.Text))
                return (null, false);

            // 4. Deserialize the JSON payload from the QR code
            var payload = JsonConvert.DeserializeObject<PayloadDto>(result.Text);
            if (payload == null || string.IsNullOrEmpty(payload.Data) || string.IsNullOrEmpty(payload.Signature))
                return (null, false);

            // 5. Recreate the signature using the Data field
            var newSignature = GenerateSignature(payload.Data);

            // 6. Compare the new signature with the one from the QR code
            if (newSignature != payload.Signature)
                return (null, false);

            // 7. Deserialize the Data field to OrderItemQrCodeDto
            var orderItem = JsonConvert.DeserializeObject<OrderItemQrCodeDto>(payload.Data);
            return (orderItem, true);
        }
        catch
        {
            return (null, false);
        }
    }

    private class PayloadDto
    {
        public string Data { get; set; }
        public string Signature { get; set; }
    }

    private string GenerateSignature(string data)
    {
        using var hmac = new HMACSHA256(System.Text.Encoding.UTF8.GetBytes(_secretKey));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}
