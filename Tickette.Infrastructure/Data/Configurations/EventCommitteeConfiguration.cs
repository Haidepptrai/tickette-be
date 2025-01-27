using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Configurations;

public class EventCommitteeConfiguration : IEntityTypeConfiguration<EventCommittee>
{
    public void Configure(EntityTypeBuilder<EventCommittee> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(10000);

        builder.Property(e => e.CreatedAt)
            .ValueGeneratedOnAdd();

        builder.HasOne(e => e.Event)
            .WithOne(c => c.Committee)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}