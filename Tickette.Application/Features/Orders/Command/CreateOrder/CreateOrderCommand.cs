using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Common;

namespace Tickette.Application.Features.Orders.Command.CreateOrder;

public record CreateOrderCommand
{
    public Guid UserId { get; set; }
    public required Guid EventId { get; init; }
    public required Guid EventDateId { get; init; }
    public required ICollection<TicketReservation> Tickets { get; set; }

    public string BuyerEmail { get; init; }
    public string BuyerName { get; init; }
    public string BuyerPhone { get; init; }
    public string? CouponCode { get; init; }

    public bool HasSeatMap { get; set; } = false;

    public string PaymentIntentId { get; set; }
}

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IReservationService _reservationService;
    private readonly IMessageRequestClient _messageRequestClient;

    public CreateOrderCommandHandler(
        IApplicationDbContext context,
        IReservationService reservationService,
        IMessageRequestClient messageRequestClient)
    {
        _context = context;
        _reservationService = reservationService;
        _messageRequestClient = messageRequestClient;
    }

    public async Task<Unit> Handle(CreateOrderCommand query, CancellationToken cancellation)
    {
        // Publish message to RabbitMQ to set reservation to confirm
        await _messageRequestClient.FireAndForgetAsync(query, cancellation);

        return Unit.Value;
    }
}