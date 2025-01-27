using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Events.Common;

public static class EventMapper
{
    public static EventListDto ToEventListDto(this Event entity) => new()
    {
        Name = entity.Name,
        Description = entity.Description,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        LocationName = entity.LocationName,
        City = entity.City,
        District = entity.District,
        Ward = entity.Ward,
        StreetAddress = entity.StreetAddress,
        ImageUrl = entity.Banner,
        Category = entity.Category.Name,
        Committee = new CommitteeInformation()
        {
            CommitteeName = entity.Committee.Name,
            CommitteeDescription = entity.Committee.Description
        }
    };

    public static EventDetailDto ToEventDetailDto(this Event entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        LocationName = entity.LocationName,
        City = entity.City,
        District = entity.District,
        Ward = entity.Ward,
        StreetAddress = entity.StreetAddress,
        Description = entity.Description,
        Logo = entity.Logo,
        Banner = entity.Banner,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = entity.Status,
        CategoryName = entity.Category.Name,
        EventCommitteeInformation = new CommitteeInformation()
        {
            CommitteeName = entity.Committee.Name,
            CommitteeDescription = entity.Committee.Description
        },
        Tickets = new List<TicketDto>()
    };
}