using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;
using Tickette.Application.Common.Interfaces;
using Tickette.Domain.Entities;
using Tickette.Domain.Enums;

namespace Tickette.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool _isAdminOrModerator;

    public DbSet<Event> Events { get; set; }
    public DbSet<EventCommittee> EventCommittees { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailUnsubscription> EmailUnsubscriptions { get; set; }
    public DbSet<EventDate> EventDates { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<ReservationItem> ReservationItems { get; set; }
    public DbSet<SeatAssignment> SeatAssignments { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CommitteeMember> CommitteeMembers { get; set; }
    public DbSet<CommitteeRole> CommitteeRoles { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
        SetQueryFilterCondition(); // Dynamically set the filter condition
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

        // Apply query filters dynamically for `Event`
        builder.Entity<Event>().HasQueryFilter(e =>
            !_isAdminOrModerator || (e.DeletedAt == null && e.Status == ApprovalStatus.Approved));
    }

    private void SetQueryFilterCondition()
    {
        var userRoles = _httpContextAccessor.HttpContext?.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Dynamically update the filter condition per request
        _isAdminOrModerator = userRoles?.Contains("Admin") == true || userRoles?.Contains("Moderator") == true;
    }
}
