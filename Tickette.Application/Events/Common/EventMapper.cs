using Tickette.Domain.Entities;

namespace Tickette.Application.Events.Common;

public static class EventMapper
{
    public static EventListDto ToEventListDto(this Event entity) => new()
    {
        Name = entity.Name,
        Description = entity.Description,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Location = entity.Address,
        ImageUrl = entity.Banner,
        EventType = entity.Type.ToString(),
        Committee = new CommitteeInformation()
        {
            Name = entity.Committee.Name,
            Description = entity.Committee.Description
        }
    };
}