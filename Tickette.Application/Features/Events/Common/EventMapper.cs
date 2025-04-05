using Tickette.Application.Features.Events.Common.Client;
using Tickette.Domain.Entities;

namespace Tickette.Application.Features.Events.Common;

public static class EventMapper
{
    public static EventListDto ToEventListDto(this Event entity) => new()
    {
        Name = entity.Name,
        Description = entity.Description,
        LocationName = entity.LocationName,
        City = entity.City,
        District = entity.District,
        Ward = entity.Ward,
        StreetAddress = entity.StreetAddress,
        ImageUrl = entity.Banner,
        Category = entity.Category.Name,
        Committee = new CommitteeInformation()
        {
            Logo = entity.Committee.Logo,
            Name = entity.Committee.Name,
            Description = entity.Committee.Description
        }
    };

    public static UserEventListResponse ToUserEventListResponse(this Event entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Status = entity.Status.ToString(),
        StartDate = entity.EventDates.Min(ed => ed.StartDate),
        EndDate = entity.EventDates.Max(ed => ed.EndDate),
        Slug = entity.EventSlug,
        Banner = entity.Banner
    };

    public static EventPreviewDto ToEventPreviewDto(this Event entity) => new()
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
        Slug = entity.EventSlug
    };

    public static EventDetailDto ToEventDetailDto(this Event entity, IEnumerable<Category>? category, CommitteeMember? currentUser) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        IsOffline = entity.IsOffline,
        LocationName = entity.LocationName,
        City = entity.City,
        District = entity.District,
        Ward = entity.Ward,
        StreetAddress = entity.StreetAddress,
        Description = entity.Description,
        Banner = entity.Banner,
        Status = entity.Status,
        CategoryId = entity.Category.Id,
        EventOwnerStripeId = entity.EventOwnerStripeId,
        CommitteeName = entity.Committee.Name,
        CommitteeDescription = entity.Committee.Description,
        CommitteeLogo = entity.Committee.Logo,
        EventDates = entity.EventDates.Select(ev => new EventDateDto()
        {
            Id = ev.Id,
            StartDate = ev.StartDate,
            EndDate = ev.EndDate,
            Tickets = ev.Tickets.Select(ticket => new TicketDto()
            {
                Id = ticket.Id,
                Name = ticket.Name,
                Description = ticket.Description,
                TotalTickets = ticket.TotalTickets,
                MinPerOrder = ticket.MinTicketsPerOrder,
                MaxPerOrder = ticket.MaxTicketsPerOrder,
                Amount = ticket.Price.Amount,
                Currency = ticket.Price.Currency,
                SaleStartTime = ticket.SaleStartTime,
                SaleEndTime = ticket.SaleEndTime,
                TicketImage = ticket.Image,
                IsFree = ticket.Price.Amount == 0
            }),
            SeatMap = ev.SeatMap
        }),
        Categories = category?.Select(c => new CategoryDto()
        {
            Id = c.Id,
            Name = c.Name
        }),
        UserInEventInfo = currentUser is null ? null : new CommitteeMemberDto()
        {
            Id = currentUser.Id,
            FullName = currentUser.User.FullName!,
            Email = currentUser.User.Email!,
            Role = currentUser.CommitteeRole.Name,
        }
    };

    public static EventDetailStatisticDto ToEventDetailStatisticDto(this Event entity)
    {
        return new EventDetailStatisticDto
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
            Status = entity.Status,
            CategoryName = entity.Category.Name,
            Committee = new CommitteeInformation()
            {
                Logo = entity.Committee.Logo,
                Name = entity.Committee.Name,
                Description = entity.Committee.Description
            },
            EventDates = entity.EventDates.Select(ev => new EventDateDto()
            {
                Id = ev.Id,
                StartDate = ev.StartDate,
                EndDate = ev.EndDate,
                Tickets = ev.Tickets.Select(ticket => new TicketDto()
                {
                    Id = ticket.Id,
                    Name = ticket.Name,
                    Description = ticket.Description,
                    TotalTickets = ticket.TotalTickets,
                    MinPerOrder = ticket.MinTicketsPerOrder,
                    MaxPerOrder = ticket.MaxTicketsPerOrder,
                    Amount = ticket.Price.Amount,
                    Currency = ticket.Price.Currency,
                    SaleStartTime = ticket.SaleStartTime,
                    SaleEndTime = ticket.SaleEndTime,
                    TicketImage = ticket.Image
                })
            }),
            EventOwnerStripeId = entity.EventOwnerStripeId,
            CommitteeMembers = entity.CommitteeMembers.Select(cm => new CommitteeMemberDto()
            {
                Id = cm.Id,
                FullName = cm.User.FullName!,
                Email = cm.User.Email!,
                Role = cm.CommitteeRole.Name,
            })
        };
    }
}