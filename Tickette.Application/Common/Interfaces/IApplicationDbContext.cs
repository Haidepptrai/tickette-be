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

    DbSet<Cart> Carts { get; set; }

    DbSet<CartItem> CartItems { get; set; }

    DbSet<Coupon> Coupons { get; set; }

    DbSet<EventSeat> EventSeats { get; set; }

    DbSet<RefreshToken> RefreshTokens { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}