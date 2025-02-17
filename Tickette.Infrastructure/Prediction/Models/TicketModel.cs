namespace Tickette.Infrastructure.Prediction.Models;

public class TicketModel
{
    public decimal Price { get; set; }

    public TicketModel(decimal price)
    {
        Price = price;
    }
}