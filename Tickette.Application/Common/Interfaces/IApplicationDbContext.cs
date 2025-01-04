using Microsoft.EntityFrameworkCore;
using Tickette.Domain.Entities;

namespace Tickette.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Event> Events { get; }

    DbSet<EventCommittee> EventCommittees { get; }

    DbSet<Ticket> Tickets { get; }

    DbSet<CommitteeMember> CommitteeMembers { get; }

    DbSet<CommitteeRole> CommitteeRoles { get; }

    DbSet<Category> Categories { get; }

    DbSet<Order> Orders { get; }

    DbSet<OrderItem> OrderItems { get; }

    DbSet<Coupon> Coupons { get; }

    DbSet<EventSeat> EventSeats { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}