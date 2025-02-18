using Tickette.Application.Features.Events.Common;

namespace Tickette.Admin.Dtos;

public class CreateEventCommandDto
{
    public required string Name { get; set; }

    public required string LocationName { get; set; }

    public required string City { get; set; }

    public required string District { get; set; }

    public required string Ward { get; set; }

    public required string StreetAddress { get; set; }

    public required Guid CategoryId { get; set; }

    public required string Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public required string CommitteeName { get; set; }

    public required string CommitteeDescription { get; set; }

    public required IFormFile LogoFile { get; set; }

    public required IFormFile BannerFile { get; set; }

    public required EventDateInput[] EventDates { get; set; }
}