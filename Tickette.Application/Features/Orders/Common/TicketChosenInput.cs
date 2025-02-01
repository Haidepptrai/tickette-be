namespace Tickette.Application.Features.Orders.Common;

public record TicketChosenInput
{
    public Guid Id { get; init; }

    public int Quantity { get; init; }
}