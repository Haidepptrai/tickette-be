using Tickette.Application.Exceptions;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Orders.Common;

public record TicketReservation
{
    public Guid Id { get; init; }

    public int Quantity { get; init; }

    public string? SectionName { get; init; }

    public IEnumerable<SeatOrder>? SeatsChosen { get; init; }

    public TicketReservation(Guid id, int quantity, string? sectionName, IEnumerable<SeatOrder>? seatsChosen)
    {
        if (quantity <= 0)
        {
            throw new InvalidQuantityException();
        }

        Id = id;
        Quantity = quantity;
        SectionName = sectionName;
        SeatsChosen = seatsChosen;
    }
}