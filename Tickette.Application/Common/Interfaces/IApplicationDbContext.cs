using Microsoft.EntityFrameworkCore;
using Tickette.Domain.Entities;

namespace Tickette.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Event> Events { get; set; }

    DbSet<EventCommittee> EventCommittees { get; set; }

    DbSet<Ticket> Tickets { get; set; }

    DbSet<CommitteeMember> CommitteeMembers { get; set; }

    DbSet<CommitteeRole> CommitteeRoles { get; set; }

    DbSet<Category> Categories { get; set; }

    DbSet<Order> Orders { get; set; }

    DbSet<OrderItem> OrderItems { get; set; }

    DbSet<Coupon> Coupons { get; set; }

    DbSet<RefreshToken> RefreshTokens { get; set; }

    DbSet<EmailUnsubscription> EmailUnsubscriptions { get; set; }

    DbSet<EventDate> EventDates { get; set; }

    DbSet<Reservation> Reservations { get; set; }

    DbSet<ReservationItem> ReservationItems { get; set; }

    DbSet<SeatAssignment> SeatAssignments { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}