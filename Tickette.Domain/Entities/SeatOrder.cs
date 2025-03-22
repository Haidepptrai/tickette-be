namespace Tickette.Domain.Entities;

public class SeatOrder
{
    public string RowName { get; init; }

    public string SeatNumber { get; init; }

    public SeatOrder() { }

    public SeatOrder(string rowName, string seatNumber)
    {
        RowName = rowName;
        SeatNumber = seatNumber;
    }


}