using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Configurations;

public class SeatAssignmentConfiguration : IEntityTypeConfiguration<SeatAssignment>
{
    public void Configure(EntityTypeBuilder<SeatAssignment> builder)
    {
        builder.Property(s => s.ItemId)
            .IsRequired();

        builder.Property(s => s.RowName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.SeatNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(s => new { s.ItemId, s.RowName, s.SeatNumber })
            .IsUnique();
    }
}
