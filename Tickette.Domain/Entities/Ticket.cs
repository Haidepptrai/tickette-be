using Tickette.Domain.Common;

namespace Tickette.Domain.Entities;

public sealed class Ticket : BaseEntity
{
    public Guid EventDateId { get; private set; }

    public string Name { get; private set; }

    public decimal Price { get; private set; }

    public int TotalTickets { get; private set; }

    public int RemainingTickets { get; private set; }

    public int MinTicketsPerOrder { get; private set; }

    public int MaxTicketsPerOrder { get; private set; }

    public DateTime SaleStartTime { get; private set; }

    public DateTime SaleEndTime { get; private set; }

    public string Description { get; private set; }

    public string? TicketImage { get; private set; }

    public EventDate EventDate { get; private set; }

    public ICollection<EventSeat>? Seats { get; set; } = new List<EventSeat>();

    protected Ticket() { }

    private Ticket(
    EventDate eventDate,
    string name,
    decimal price,
    int totalTickets,
    int minTicketsPerOrder,
    int maxTicketsPerOrder,
    DateTime saleStartTime,
    DateTime saleEndTime,
    string description,
    string? ticketImage)
    {
        EventDate = eventDate;
        Name = name;
        Price = price;
        TotalTickets = totalTickets;
        RemainingTickets = totalTickets;
        MinTicketsPerOrder = minTicketsPerOrder;
        MaxTicketsPerOrder = maxTicketsPerOrder;
        SaleStartTime = saleStartTime;
        SaleEndTime = saleEndTime;
        Description = description;
        TicketImage = ticketImage;
    }

    public static Ticket Create(
        EventDate eventDate,
        string name,
        decimal price,
        int totalTickets,
        int minTicketsPerOrder,
        int maxTicketsPerOrder,
        DateTime saleStartTime,
        DateTime saleEndTime,
        string description,
        string? ticketImage)
    {
        // Business rule validations
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.");

        if (price < 0)
            throw new ArgumentException("Price must be a positive value.");

        if (totalTickets <= 0)
            throw new ArgumentException("Total tickets must be greater than zero.");

        if (minTicketsPerOrder < 0 || maxTicketsPerOrder < minTicketsPerOrder)
            throw new ArgumentException("Invalid ticket order limits.");

        if (saleStartTime >= saleEndTime)
            throw new ArgumentException("Sale start time must be before sale end time.");

        // Create and return a valid Ticket object
        return new Ticket(
            eventDate,
            name,
            price,
            totalTickets,
            minTicketsPerOrder,
            maxTicketsPerOrder,
            saleStartTime,
            saleEndTime,
            description,
            ticketImage);
    }


    public void ReduceTickets(int quantity)
    {
        if (quantity > RemainingTickets)
            throw new ArgumentException("Quantity exceeds remaining tickets.");
        RemainingTickets -= quantity;
    }
}