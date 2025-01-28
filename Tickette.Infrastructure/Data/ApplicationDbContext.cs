using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Tickette.Application.Common.Interfaces;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public DbSet<Event> Events { get; set; }
    public DbSet<EventCommittee> EventCommittees { get; set; }

    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<EventSeat> EventSeats { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Category> Categories { get; set; }
    public DbSet<CommitteeMember> CommitteeMembers { get; set; }
    public DbSet<CommitteeRole> CommitteeRoles { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Entity<User>(b => b.ToTable("identity_users"));
        builder.Entity<IdentityRole<Guid>>(b => b.ToTable("identity_roles"));
        builder.Entity<IdentityUserRole<Guid>>(b => b.ToTable("identity_user_roles"));
        builder.Entity<IdentityUserClaim<Guid>>(b => b.ToTable("identity_user_claims"));
        builder.Entity<IdentityUserLogin<Guid>>(b => b.ToTable("identity_user_logins"));
        builder.Entity<IdentityRoleClaim<Guid>>(b => b.ToTable("identity_role_claims"));
        builder.Entity<IdentityUserToken<Guid>>(b => b.ToTable("identity_user_tokens"));
    }

}