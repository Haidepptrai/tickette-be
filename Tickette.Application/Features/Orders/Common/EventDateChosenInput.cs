namespace Tickette.Application.Features.Orders.Common;

public record EventDateChosenInput
{
    public Guid Id { get; set; }

    public DateTime EventDate { get; set; }

    public ICollection<TicketChosenInput> ChosenTickets { get; set; }
}