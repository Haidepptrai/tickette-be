using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Features.QRCode.Common;
using Tickette.Application.Helpers;

namespace Tickette.Application.Features.QRCode.Queries;

public record GetQrCodeQuery
{
    public Guid UserId { get; init; }
    public Guid OrderItemId { get; init; }
}

public class GetQrCodeQueryQueryHandler : IQueryHandler<GetQrCodeQuery, ResponseDto<byte[]>>
{
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeService _qrCodeService;

    public GetQrCodeQueryQueryHandler(IApplicationDbContext context, IQrCodeService qrCodeService)
    {
        _context = context;
        _qrCodeService = qrCodeService;
    }

    public async Task<ResponseDto<byte[]>> Handle(GetQrCodeQuery query, CancellationToken cancellation)
    {

        var orderItem = await _context.OrderItems
            .Include(oi => oi.Order)
                .ThenInclude(o => o.Items)
            .Include(oi => oi.Ticket)
                .ThenInclude(t => t.Seats)
            .FirstOrDefaultAsync(oi => oi.Id == query.OrderItemId, cancellation);

        if (orderItem == null)
        {
            throw new KeyNotFoundException("Not Found Order Item");
        }


        var orderItemDto = new OrderItemQrCodeDto
        {
            BuyerEmail = orderItem.Order.BuyerEmail,
            BuyerName = orderItem.Order.BuyerName,
            BuyerPhone = orderItem.Order.BuyerPhone,
            OrderItemId = orderItem.Id,
            EventId = orderItem.Order.EventId,
            TicketName = orderItem.Ticket.Name,
            TicketEventStartTime = orderItem.Ticket.EventStartTime,
            TicketEventEndTime = orderItem.Ticket.EventEndTime,
            SeatsOrdered = orderItem.Seats
        };

        var qrCode = _qrCodeService.GenerateQrCode(orderItemDto);

        return ResponseHandler.SuccessResponse(qrCode, "Get QR Code For Order Successfully");

    }
}