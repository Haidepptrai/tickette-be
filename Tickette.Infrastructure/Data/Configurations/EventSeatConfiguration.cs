using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Configurations;

public class EventSeatConfiguration : IEntityTypeConfiguration<EventSeat>
{
    public void Configure(EntityTypeBuilder<EventSeat> builder)
    {
        builder.Property(es => es.Row)
            .IsRequired();

        builder.Property(es => es.Column)
            .IsRequired();

        builder.Property(es => es.IsAvailable)
            .IsRequired();

        builder.HasIndex(es => new { es.Row, es.Column, es.EventId })
            .IsUnique();

        builder.HasOne(es => es.Event)
            .WithMany(e => e.Seats)
            .HasForeignKey(es => es.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(es => es.Ticket)
            .WithMany(t => t.Seats)
            .OnDelete(DeleteBehavior.Cascade);
    }
}