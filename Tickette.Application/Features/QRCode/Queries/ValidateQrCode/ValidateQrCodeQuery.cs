using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Exceptions;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.QRCode.Common;
using Tickette.Application.Wrappers;

namespace Tickette.Application.Features.QRCode.Queries.ValidateQrCode;

public record ValidateQrCodeQuery
{
    public required string QrCode { get; init; }
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
            var (orderDetails, isValid) = _qrCodeService.DecodeQrCode(Convert.FromBase64String(query.QrCode));

            if (!isValid || orderDetails == null)
            {
                return ResponseHandler.ErrorResponse(
                    new DataRetrievedFromQrCode { IsValid = false },
                    "Invalid QR Code"
                );
            }

            // Update order item IsScanned to true
            var orderItem = await _context.OrderItems.FirstOrDefaultAsync(x => x.Id == orderDetails.OrderItemId, cancellation);

            if (orderItem == null)
            {
                return ResponseHandler.ErrorResponse(
                    new DataRetrievedFromQrCode { IsValid = false },
                    "Order item not found"
                );
            }

            // if the order item is already scanned, return an error
            if (orderItem.IsScanned)
            {
                throw new QrCodeAlreadyScannedException();
            }

            orderItem.SetAsScanned();

            var responseDto = new DataRetrievedFromQrCode()
            {

                IsValid = true,
                OrderDetail = new OrderDetailFromQrCodeDto(orderDetails.EventId, orderDetails.EventId,
                    orderDetails.SeatsOrdered?.Select(seat => seat.Id).ToList())
            };

            await _context.SaveChangesAsync(cancellation);

            return ResponseHandler.SuccessResponse(responseDto, "QR Code validated successfully.");
        }
        catch (QrCodeAlreadyScannedException ex)
        {
            throw new QrCodeAlreadyScannedException();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to decode QR code", ex);
        }
    }
}