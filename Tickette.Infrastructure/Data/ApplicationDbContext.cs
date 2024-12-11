using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Tickette.Application.Common.Interfaces;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User>, IApplicationDbContext
{
    public DbSet<Event> Events { get; set; }
    public DbSet<EventCommittee> EventCommittees { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Entity<User>(b => b.ToTable("identity_users"));
        builder.Entity<IdentityRole>(b => b.ToTable("identity_roles"));
        builder.Entity<IdentityUserRole<string>>(b => b.ToTable("identity_user_roles"));
        builder.Entity<IdentityUserClaim<string>>(b => b.ToTable("identity_user_claims"));
        builder.Entity<IdentityUserLogin<string>>(b => b.ToTable("identity_user_logins"));
        builder.Entity<IdentityRoleClaim<string>>(b => b.ToTable("identity_role_claims"));
        builder.Entity<IdentityUserToken<string>>(b => b.ToTable("identity_user_tokens"));

    }
}