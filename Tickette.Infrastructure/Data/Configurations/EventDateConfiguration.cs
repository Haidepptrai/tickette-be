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
        builder.Property(ed => ed.StartDate)
            .IsRequired();

        builder.Property(ed => ed.EndDate)
            .IsRequired();

        builder.HasOne(ed => ed.Event)
            .WithMany(ed => ed.EventDates)
            .HasForeignKey(ed => ed.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(ed => ed.SeatMap)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, SerializationSettings.JsonOptions),
                v => JsonSerializer.Deserialize<EventSeatMap>(v, SerializationSettings.JsonOptions) ??
                     EventSeatMap.CreateEventSeatMap(new List<EventSeatMapSection>(), new List<TicketSeatMapping>())
            );

        builder.HasIndex(ed => ed.SeatMap)
            .HasMethod("GIN");

        builder.HasQueryFilter(ed => ed.DeletedAt == null);
    }
}