using Tickette.Application.Features.Events.Common;

namespace Tickette.API.DTOs;

public class UpdateEventCommandDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? LocationName { get; set; } = string.Empty;

    public string? City { get; set; } = string.Empty;

    public string? District { get; set; } = string.Empty;

    public string? Ward { get; set; } = string.Empty;

    public string? StreetAddress { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }

    public string Description { get; set; }

    public IFormFile? CommitteeLogo { get; set; }

    public string? CommitteeLogoUrl { get; set; }

    public string CommitteeName { get; set; }

    public string CommitteeDescription { get; set; }

    public string EventOwnerStripeId { get; set; }

    public bool IsOffline { get; set; }

    public IFormFile? BannerFile { get; set; }

    public string? BannerUrl { get; set; }

    public EventDateInputForUpdate[] EventDates { get; set; }
}