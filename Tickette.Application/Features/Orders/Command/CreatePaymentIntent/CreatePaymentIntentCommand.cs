using Microsoft.EntityFrameworkCore;
using Tickette.Application.Common;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Stripe;
using Tickette.Application.Exceptions;
using Tickette.Application.Features.Orders.Common;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Orders.Command.CreatePaymentIntent;

public record CreatePaymentIntentCommand
{
    public required ICollection<TicketReservation> Tickets { get; set; }

    public required string EventOwnerStripeId { get; init; }
}

public class CreatePaymentIntentCommandHandler : ICommandHandler<CreatePaymentIntentCommand, PaymentIntentResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentService _paymentService;

    public CreatePaymentIntentCommandHandler(IApplicationDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task<PaymentIntentResult> Handle(CreatePaymentIntentCommand command, CancellationToken cancellation)
    {
        decimal totalPrice = 0;

        // Validate Tickets and Calculate Total Price
        foreach (var ticket in command.Tickets)
        {
            var ticketInfo = await _context.Tickets
                .Where(t => t.Id == ticket.Id)
                .Select(t => new { t.Price, t.RemainingTickets })
                .SingleOrDefaultAsync(cancellation);

            if (ticketInfo == null)
            {
                throw new NotFoundException("Ticket", ticket.Id);
            }

            if (ticket.Quantity <= 0 || ticket.Quantity > ticketInfo.RemainingTickets)
            {
                throw new InvalidQuantityException();
            }

            totalPrice += ticketInfo.Price * ticket.Quantity;
        }

        if (totalPrice <= 0)
        {
            throw new InvalidOperationException("An error occurred while calculate pricing.");
        }

        var payment = Payment.Create(totalPrice, command.EventOwnerStripeId);
        var paymentIntentResult = await _paymentService.CreatePaymentIntentAsync(payment);

        return new PaymentIntentResult
        {
            PaymentIntentId = paymentIntentResult.PaymentIntentId,
            ClientSecret = paymentIntentResult.ClientSecret,
        };
    }
}