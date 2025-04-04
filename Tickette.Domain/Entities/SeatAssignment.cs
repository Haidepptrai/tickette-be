namespace Tickette.Domain.Entities;

public class SeatAssignment
{
    public Guid Id { get; private set; }
    public Guid ItemId { get; private set; }
    public string RowName { get; private set; }
    public string SeatNumber { get; private set; }

    private SeatAssignment() { }

    public SeatAssignment(Guid itemId, string rowName, string seatNumber)
    {
        Id = Guid.NewGuid();
        ItemId = itemId;
        RowName = rowName;
        SeatNumber = seatNumber;
    }
}
