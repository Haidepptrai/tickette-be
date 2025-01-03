using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasOne(t => t.Event)
            .WithMany(e => e.Tickets)
            .HasForeignKey(t => t.EventId);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(t => t.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.TotalTickets)
            .IsRequired();

        builder.Property(t => t.RemainingTickets)
            .IsRequired();

        builder.Property(t => t.MinTicketsPerOrder)
            .IsRequired();

        builder.Property(t => t.MaxTicketsPerOrder)
            .IsRequired();

        builder.Property(t => t.SaleStartTime)
            .IsRequired();

        builder.Property(t => t.SaleEndTime)
            .IsRequired();

        builder.Property(t => t.EventStartTime)
            .IsRequired();

        builder.Property(t => t.EventEndTime)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(1500);

        builder.Property(t => t.TicketImage)
            .HasMaxLength(2500);
    }
}
