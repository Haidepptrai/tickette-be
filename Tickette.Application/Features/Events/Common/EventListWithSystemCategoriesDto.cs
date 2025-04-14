namespace Tickette.Application.Features.Events.Common;

public class EventListWithSystemCategoriesDto : EventListDto
{
    public IEnumerable<EventListDto> Events { get; set; } = new List<EventListDto>();
    public IEnumerable<CategoryDto> SystemCategories { get; set; } = new List<CategoryDto>();
}