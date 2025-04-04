namespace Tickette.Domain.Entities;

public class ReservationItem
{
    public Guid Id { get; private set; }
    public Guid ReservationId { get; private set; }
    public Guid TicketId { get; private set; }
    public int Quantity { get; private set; }
    public bool HasAssignedSeats { get; private set; }

    private readonly List<SeatAssignment> _seatAssignments = new();
    public IReadOnlyCollection<SeatAssignment> SeatAssignments => _seatAssignments.AsReadOnly();

    public Reservation Reservation { get; set; }

    private ReservationItem() { }

    public ReservationItem(Guid reservationId, Guid ticketId, int quantity, bool hasAssignedSeats)
    {
        Id = Guid.NewGuid();
        ReservationId = reservationId;
        TicketId = ticketId;
        Quantity = quantity;
        HasAssignedSeats = hasAssignedSeats;
    }

    public void AssignSeat(string rowName, string seatNumber)
    {
        if (!HasAssignedSeats)
            throw new Exception("Cannot assign seats to general admission tickets.");

        _seatAssignments.Add(new SeatAssignment(Id, rowName, seatNumber));
    }
}
