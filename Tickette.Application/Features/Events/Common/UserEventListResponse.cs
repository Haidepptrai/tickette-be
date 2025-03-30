namespace Tickette.Application.Features.Events.Common;

public record UserEventListResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Status { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Slug { get; set; }

    public string Banner { get; set; }
}