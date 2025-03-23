namespace Tickette.Application.Features.Events.Common.Admin;

public record EventsStatisticDto
{
    public int PendingEvents { get; init; }
    public int ApprovedEvents { get; init; }
    public int RejectedEvents { get; init; }
    public int UpComingEvents { get; init; }
}