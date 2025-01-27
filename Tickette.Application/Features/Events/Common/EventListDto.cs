namespace Tickette.Application.Features.Events.Common;

public class EventListDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string LocationName { get; set; }

    public string City { get; set; }

    public string District { get; set; }

    public string Ward { get; set; }

    public string StreetAddress { get; set; }

    public string ImageUrl { get; set; }

    public string Category { get; set; }

    public CommitteeInformation Committee { get; set; }
}