namespace Tickette.Application.Features.Events.Common;

public record EventPreviewDto
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string LocationName { get; set; }

    public string City { get; set; }

    public string District { get; set; }

    public string Ward { get; set; }

    public string StreetAddress { get; set; }

    public required string Description { get; set; }

    public required string Banner { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string CategoryName { get; set; }

    public string Slug { get; set; }
}