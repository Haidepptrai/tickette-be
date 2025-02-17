using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.QRCode.Common;
using Tickette.Application.Wrappers;

namespace Tickette.Application.Features.QRCode.Queries.ValidateQrCode;

public record ValidateQrCodeQuery
{
    public required Guid EventId { get; init; }
    public required string BuyerEmail { get; init; }

    public required string BuyerName { get; init; }

    public required string BuyerPhone { get; init; }

    public required Guid OrderId { get; init; }

    public required Guid OrderItemId { get; init; }

    public required string Signature { get; init; }
}

public class ValidateQrCodeQueryHandler : IQueryHandler<ValidateQrCodeQuery, ResponseDto<DataRetrievedFromQrCode>>
{
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeService _qrCodeService;

    public ValidateQrCodeQueryHandler(IQrCodeService qrCodeService, IApplicationDbContext context)
    {
        _qrCodeService = qrCodeService;
        _context = context;
    }

    public async Task<ResponseDto<DataRetrievedFromQrCode>> Handle(ValidateQrCodeQuery query, CancellationToken cancellation)
    {
        try
        {
            // 1. Concatenate data fields into a single string for verification
            var dataToVerify = _qrCodeService.SerializeData(query);

            // 2. Validate the QR Code Signature using QrCodeService
            if (!_qrCodeService.VerifyQrCodeSignature(dataToVerify, query.Signature))
            {
                return ResponseHandler.ErrorResponse(new DataRetrievedFromQrCode { IsValid = false }, "Invalid QR Code Signature.");
            }

            // 3. Check if order item exists
            var orderItem = await _context.OrderItems.FirstOrDefaultAsync(x => x.Id == query.OrderItemId, cancellation);

            if (orderItem == null)
            {
                return ResponseHandler.ErrorResponse(new DataRetrievedFromQrCode { IsValid = false }, "Order item not found.");
            }

            // 4. Prevent duplicate scans
            if (orderItem.IsScanned)
            {
                return ResponseHandler.ErrorResponse(new DataRetrievedFromQrCode { IsValid = false }, "QR Code already scanned.");
            }

            // 5. Mark ticket as scanned
            orderItem.SetAsScanned();
            await _context.SaveChangesAsync(cancellation);

            // 6. Return success response
            var responseDto = new DataRetrievedFromQrCode()
            {
                IsValid = true,
                OrderDetail = new OrderDetailFromQrCodeDto(query.OrderId, query.OrderItemId, null)
            };

            return ResponseHandler.SuccessResponse(responseDto, "QR Code validated successfully.");
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to validate QR code", ex);
        }
    }
}
