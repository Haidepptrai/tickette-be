using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Events.Common.Admin;

public static class EventMapper
{
    public static AdminEventPreviewDto ToEventPreviewDto(this Event entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        LocationName = entity.LocationName,
        City = entity.City,
        District = entity.District,
        Ward = entity.Ward,
        StreetAddress = entity.StreetAddress,
        Description = entity.Description,
        Banner = entity.Banner,
        CategoryName = entity.Category.Name,
        Slug = entity.EventSlug,
        Status = entity.Status,
        StartDate = entity.EventDates.Min(ed => ed.StartDate),
        EndDate = entity.EventDates.Max(ed => ed.EndDate),
    };

}