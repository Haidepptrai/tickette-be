﻿using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.QRCode.Common;
using Tickette.Application.Wrappers;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.QRCode.Queries.AdminCheckQrCodeFraud;

public class AdminCheckQrCodeFraudQuery
{
    public string BuyerEmail { get; init; }

    public string BuyerName { get; init; }

    public string BuyerPhone { get; init; }

    public Guid OrderId { get; init; }

    public Guid OrderItemId { get; init; }

    public string Signature { get; init; }

    public ICollection<SeatOrder>? SeatsOrdered { get; init; }
}

public class AdminCheckQrCodeFraudQueryHandler : IQueryHandler<AdminCheckQrCodeFraudQuery, ResponseDto<DataRetrievedFromQrCode>>
{
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeService _qrCodeService;

    public AdminCheckQrCodeFraudQueryHandler(IQrCodeService qrCodeService, IApplicationDbContext context)
    {
        _qrCodeService = qrCodeService;
        _context = context;
    }

    public async Task<ResponseDto<DataRetrievedFromQrCode>> Handle(AdminCheckQrCodeFraudQuery query, CancellationToken cancellation)
    {
        // 1. Concatenate data fields into a single string for verification
        var toOrderData = new OrderItemQrCodeDto()
        {
            BuyerEmail = query.BuyerEmail,
            BuyerName = query.BuyerName,
            BuyerPhone = query.BuyerPhone,
            OrderId = query.OrderId,
            OrderItemId = query.OrderItemId,
            SeatsOrdered = query.SeatsOrdered
        };

        var dataToVerify = _qrCodeService.SerializeData(toOrderData);

        // 2. Validate the QR Code Signature using QrCodeService
        if (!_qrCodeService.VerifyQrCodeSignature(dataToVerify, query.Signature))
        {
            return ResponseHandler.ErrorResponse(new DataRetrievedFromQrCode { IsValid = false },
                "Invalid QR Code Signature.");
        }

        // 3. Check if order item exists
        var orderItem = await _context.OrderItems.FirstOrDefaultAsync(x => x.Id == query.OrderItemId, cancellation);

        if (orderItem == null)
        {
            return ResponseHandler.ErrorResponse(new DataRetrievedFromQrCode { IsValid = false },
                "Order item not found.");
        }

        var responseDto = new DataRetrievedFromQrCode()
        {
            IsValid = true,
            OrderDetail = new OrderDetailFromQrCodeDto(query.OrderId, query.OrderItemId, null)
        };

        return ResponseHandler.SuccessResponse(responseDto, "QR Code validated successfully.");
    }
}