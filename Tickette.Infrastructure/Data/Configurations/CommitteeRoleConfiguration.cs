using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Configurations;

public class CommitteeRoleConfiguration : IEntityTypeConfiguration<CommitteeRole>
{
    public void Configure(EntityTypeBuilder<CommitteeRole> builder)
    {
        builder.Property(role => role.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Configure Permissions as Owned Entity
        builder.OwnsMany(role => role.Permissions, permissions =>
        {
            permissions.WithOwner().HasForeignKey("CommitteeRoleId");
            permissions.Property(p => p.Name).IsRequired().HasMaxLength(100);
            permissions.ToTable("committee_role_permissions"); // Stored in a separate table
        });

        // Indexes
        builder.HasIndex(role => role.Name)
            .IsUnique();
    }
}