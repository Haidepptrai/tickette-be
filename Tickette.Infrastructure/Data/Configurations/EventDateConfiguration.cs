using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using Tickette.Domain.Entities;
using Tickette.Infrastructure.Persistence;

namespace Tickette.Infrastructure.Data.Configurations;

public class EventDateConfiguration : IEntityTypeConfiguration<EventDate>
{
    public void Configure(EntityTypeBuilder<EventDate> builder)
    {
        builder.Property(e => e.StartDate)
            .IsRequired();

        builder.Property(e => e.EndDate)
            .IsRequired();

        builder.HasOne(e => e.Event)
            .WithMany(e => e.EventDates)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.SeatMap)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, SerializationSettings.JsonOptions),
                v => JsonSerializer.Deserialize<EventSeatMap>(v, SerializationSettings.JsonOptions) ??
                     EventSeatMap.CreateEventSeatMap(new List<EventSeatMapSection>(), new List<TicketSeatMapping>())
            );

        builder.HasIndex(ed => ed.SeatMap)
            .HasMethod("GIN");

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}