using Tickette.Application.Exceptions;

namespace Tickette.Application.Features.Orders.Common;

public record TicketReservation
{
    public Guid Id { get; init; }

    public int Quantity { get; init; }

    public string? SectionName { get; init; }

    public TicketReservation(Guid id, int quantity, string? sectionName)
    {
        if (quantity <= 0)
        {
            throw new InvalidQuantityException();
        }

        Id = id;
        Quantity = quantity;
        SectionName = sectionName;
    }
}