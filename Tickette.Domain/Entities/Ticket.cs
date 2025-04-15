using Tickette.Application.Exceptions;
using Tickette.Domain.Common;
using Tickette.Domain.ValueObjects;

namespace Tickette.Domain.Entities;

public sealed class Ticket : BaseEntity
{
    public Guid EventDateId { get; private set; }

    public string Name { get; private set; }

    public Price Price { get; private set; }

    public int TotalTickets { get; private set; }

    public int RemainingTickets { get; private set; }

    public int MinTicketsPerOrder { get; private set; }

    public int MaxTicketsPerOrder { get; private set; }

    public DateTime SaleStartTime { get; private set; }

    public DateTime SaleEndTime { get; private set; }

    public string Description { get; private set; }

    public string? Image { get; private set; }

    public EventDate EventDate { get; private set; }

    protected Ticket() { }

    private Ticket(
    EventDate eventDate,
    string name,
    Price price,
    int totalTickets,
    int minTicketsPerOrder,
    int maxTicketsPerOrder,
    DateTime saleStartTime,
    DateTime saleEndTime,
    string description,
    string? image)
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
        Image = image;
    }

    public static Ticket Create(
        EventDate eventDate,
        string name,
        Price price,
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

        if (totalTickets <= 0)
            throw new ArgumentException("Total tickets must be greater than zero.");

        if (minTicketsPerOrder < 0 || maxTicketsPerOrder < minTicketsPerOrder)
            throw new ArgumentException("Invalid ticket order limits.");

        if (saleStartTime >= saleEndTime)
            throw new ArgumentException($"Ticket {name} Error: Sale start time must be before sale end time.");

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

    public void Update(
        string name,
        decimal amount,
        string currency,
        int totalTickets,
        int minPerOrder,
        int maxPerOrder,
        DateTime saleStartTime,
        DateTime saleEndTime,
        string description,
        string? ticketImageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.");

        if (totalTickets <= 0)
            throw new ArgumentException("Total tickets must be greater than zero.");

        if (minPerOrder < 0 || maxPerOrder < minPerOrder)
            throw new ArgumentException("Invalid ticket order limits.");

        if (saleStartTime >= saleEndTime)
            throw new ArgumentException($"Ticket {name} Error: Sale start time must be before sale end time.");

        Name = name;
        Price = Price.Create(amount, currency); // 🧠 Immutable value object reassignment
        TotalTickets = totalTickets;
        MinTicketsPerOrder = minPerOrder;
        MaxTicketsPerOrder = maxPerOrder;
        SaleStartTime = saleStartTime;
        SaleEndTime = saleEndTime;
        Description = description;

        if (!string.IsNullOrWhiteSpace(ticketImageUrl))
        {
            Image = ticketImageUrl;
        }
    }

    public void ReduceTickets(int quantity)
    {
        if (quantity > RemainingTickets)
            throw new ArgumentException("Quantity exceeds remaining tickets.");
        RemainingTickets -= quantity;
    }

    public void IncreaseTickets(int quantity)
    {
        RemainingTickets += quantity;
    }

    public void ValidateTicket(int quantity)
    {
        if (quantity <= 0 || quantity < MinTicketsPerOrder || quantity > MaxTicketsPerOrder)
            throw new InvalidQuantityException();
    }
}