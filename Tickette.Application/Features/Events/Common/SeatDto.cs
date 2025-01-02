namespace Tickette.Application.Features.Events.Common;

public class SeatDto
{
    public int Row { get; set; }
    public int Column { get; set; }
    public Guid TicketId { get; set; }
}