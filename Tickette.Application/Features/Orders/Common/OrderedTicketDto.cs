namespace Tickette.Application.Features.Orders.Common;

public record OrderedTicketDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string? Image { get; set; }

    public string? Description { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public string QrCode { get; set; }
}