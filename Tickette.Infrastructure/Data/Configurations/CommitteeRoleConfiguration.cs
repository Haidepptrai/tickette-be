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

        // Indexes
        builder.HasIndex(role => role.Name)
            .IsUnique();

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}