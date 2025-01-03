using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public class Ticket : BaseEntity
{
    public Guid EventId { get; private set; }

    public string Name { get; private set; }

    public decimal Price { get; private set; }

    public int TotalTickets { get; private set; }

    public int RemainingTickets { get; private set; }

    public int MinTicketsPerOrder { get; private set; }

    public int MaxTicketsPerOrder { get; private set; }

    public DateTime SaleStartTime { get; private set; }

    public DateTime SaleEndTime { get; private set; }

    public DateTime EventStartTime { get; private set; }

    public DateTime EventEndTime { get; private set; }

    public string Description { get; private set; }

    public string? TicketImage { get; private set; }

    public Event Event { get; set; }

    public ICollection<EventSeat>? Seats { get; set; } = new List<EventSeat>();

    protected Ticket() { }

    public Ticket(
        Guid eventId,
        string name,
        decimal price,
        int totalTickets,
        int minTicketsPerOrder,
        int maxTicketsPerOrder,
        DateTime saleStartTime,
        DateTime saleEndTime,
        DateTime eventStartTime,
        DateTime eventEndTime,
        string description,
        string? ticketImage)
    {
        EventId = eventId;
        Name = name;
        Price = price;
        TotalTickets = totalTickets;
        RemainingTickets = totalTickets;
        MinTicketsPerOrder = minTicketsPerOrder;
        MaxTicketsPerOrder = maxTicketsPerOrder;
        SaleStartTime = saleStartTime;
        SaleEndTime = saleEndTime;
        EventStartTime = eventStartTime;
        EventEndTime = eventEndTime;
        Description = description;
        TicketImage = ticketImage;
    }

    public void UpdateRemainingTickets(int quantity)
    {
        if (quantity > RemainingTickets)
            throw new ArgumentException("Quantity exceeds remaining tickets.");
        RemainingTickets -= quantity;
    }
}