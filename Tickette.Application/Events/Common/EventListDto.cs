namespace Tickette.Application.Events.Common;

public class EventListDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Location { get; set; }

    public string ImageUrl { get; set; }

    public string EventType { get; set; }

    public CommitteeInformation Committee { get; set; }
}