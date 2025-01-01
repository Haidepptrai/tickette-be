namespace Tickette.Application.Features.Tickets.Common;

public class ResponseTicketOrderedDto
{
    public Guid EventId { get; set; }

    public ResponseTicketOrderedDto(Guid eventId)
    {
        EventId = eventId;
    }
}