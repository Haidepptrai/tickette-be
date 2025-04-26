namespace Tickette.Application.Features.Events.Common.Client;

public class RecentPurchaseDto
{
    public Guid Id { get; init; }
    public string TicketName { get; init; }
    public int Quantity { get; init; }
    public decimal TotalPrice { get; init; }
    public string Currency { get; init; }
    public DateTime PurchaseDate { get; init; }
    public string BuyerEmail { get; init; }
    public string BuyerName { get; init; }
    public string BuyerPhone { get; init; }
    public IEnumerable<PurchasedTicketDetailDto> Tickets { get; init; }
}

public class PurchasedTicketDetailDto
{
    public string Name { get; init; }
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; }
    public IEnumerable<SeatInfoDto>? Seats { get; init; }
}

public class SeatInfoDto
{
    public string RowName { get; init; }
    public string SeatNumber { get; init; }
}