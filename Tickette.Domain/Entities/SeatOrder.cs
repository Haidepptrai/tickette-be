namespace Tickette.Domain.Entities;

public class SeatOrder : IEquatable<SeatOrder>
{
    public string RowName { get; init; }

    public string SeatNumber { get; init; }

    public SeatOrder() { }

    public SeatOrder(string rowName, string seatNumber)
    {
        RowName = rowName;
        SeatNumber = seatNumber;
    }

    public bool Equals(SeatOrder? other)
    {
        if (other == null) return false;
        return RowName == other.RowName && SeatNumber == other.SeatNumber;
    }

    public override int GetHashCode() => HashCode.Combine(RowName, SeatNumber);
}