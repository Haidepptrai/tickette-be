using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Configurations;

public class ReservationItemConfiguration : IEntityTypeConfiguration<ReservationItem>
{
    public void Configure(EntityTypeBuilder<ReservationItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ReservationId)
            .IsRequired();

        builder.Property(i => i.TicketId)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.HasAssignedSeats)
            .IsRequired();

        builder.HasMany(i => i.SeatAssignments)
            .WithOne()
            .HasForeignKey(s => s.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Reservation)
            .WithMany(r => r.Items)
            .HasForeignKey(i => i.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
