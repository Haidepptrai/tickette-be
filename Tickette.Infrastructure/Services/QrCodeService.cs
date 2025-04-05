using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.Orders.Common;
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

    public TicketQrCode GenerateQrCode(OrderItemQrCodeDto order, int pixelSize = 20)
    {
        // 1. Serialize the OrderItemQrCodeDto to JSON
        var jsonData = JsonConvert.SerializeObject(order);

        // 2. Generate a HMAC signature using the secret key
        var signature = GenerateSignature(jsonData);

        // 
        var qrCodeWithSignatureData = new TicketQrCode
        {
            BuyerEmail = order.BuyerEmail,
            BuyerName = order.BuyerName,
            BuyerPhone = order.BuyerPhone,
            OrderId = order.OrderId,
            OrderItemId = order.OrderItemId,
            SeatsOrdered = order.SeatsOrdered,
            Signature = signature,
        };

        // 7. Convert bytes to Base64 string
        return qrCodeWithSignatureData;
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
