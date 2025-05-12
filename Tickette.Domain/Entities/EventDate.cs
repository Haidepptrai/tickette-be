using Tickette.Domain.Common;
using Tickette.Domain.Common.Exceptions;

namespace Tickette.Domain.Entities;

public class EventDate : BaseEntity
{
    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public Guid EventId { get; private set; }

    public Event Event { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public EventSeatMap? SeatMap { get; set; }

    private EventDate() { }

    public EventDate(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static EventDate CreateEventDate(DateTime startDate, DateTime endDate)
    {
        var dateCreated = new EventDate(startDate, endDate);
        return dateCreated;
    }

    public void AddTickets(ICollection<Ticket> tickets)
    {
        Tickets = tickets;
    }

    public void AddSeatMap(EventSeatMap seatMap)
    {
        SeatMap = seatMap;
    }

    public void Update(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public void ValidateSelection(EventSeatMap seatMap, ICollection<SeatOrder> selectedSeats)
    {
        var allSeats = seatMap.GetAllSeats().ToList() ?? new List<EventSeat>();

        var groupedByRow = allSeats
            .GroupBy(seat => new { seat.RowName, seat.GroupId });

        foreach (var rowGroup in groupedByRow)
        {
            var sortedSeats = rowGroup
                .OrderBy(seat => seat.X)
                .ToList();

            // Filter to seats that are not yet ordered
            var bookableSeats = sortedSeats
                .Where(s => !s.IsOrdered)
                .ToList();

            // Filter only selected seats in this row
            var selectedInRow = selectedSeats
                .Where(sel => sel.RowName == rowGroup.Key.RowName)
                .Distinct()
                .Select(sel => sel.SeatNumber)
                .ToHashSet();

            if (!selectedInRow.Any())
                continue;

            int firstSelectedIndex = bookableSeats.FindIndex(s => selectedInRow.Contains(s.Number));
            int lastSelectedIndex = bookableSeats.FindLastIndex(s => selectedInRow.Contains(s.Number));

            if (firstSelectedIndex == -1 || lastSelectedIndex == -1)
                continue;

            // Check for unselected seats between first and last
            for (int i = firstSelectedIndex + 1; i < lastSelectedIndex; i++)
            {
                var seat = bookableSeats[i];
                if (!selectedInRow.Contains(seat.Number))
                    throw new InvalidSeatLogicSelection($"You cannot leave seat {seat.Number} alone between selected seats.");
            }

            // Check left edge
            if (firstSelectedIndex == 1)
            {
                throw new InvalidSeatLogicSelection($"You are not allowed to leave one seat on the edge.");
            }

            // Check right edge
            if (lastSelectedIndex == bookableSeats.Count - 2 &&
                !selectedInRow.Contains(bookableSeats.Last().Number))
            {
                throw new InvalidSeatLogicSelection($"You are not allowed to leave one seat on the right edge.");
            }
        }
    }
}