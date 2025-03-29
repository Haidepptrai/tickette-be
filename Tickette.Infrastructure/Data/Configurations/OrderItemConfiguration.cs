using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using Tickette.Domain.Entities;
using Tickette.Infrastructure.Persistence;

namespace Tickette.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(e => e.Quantity)
            .IsRequired();

        builder.OwnsOne(oi => oi.Price, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnType("decimal(18, 4)")
                .IsRequired();

            price.Property(p => p.Currency)
                .IsRequired();
        });

        builder.Property(e => e.IsScanned)
            .IsRequired();

        builder.Property(oi => oi.SeatsOrdered)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, SerializationSettings.JsonOptions),
                v => JsonSerializer.Deserialize<List<SeatOrder>>(v, SerializationSettings.JsonOptions)
                     ?? new List<SeatOrder>());

        builder.HasOne(o => o.Order)
            .WithMany(order => order.Items)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Ticket)
            .WithMany()
            .HasForeignKey(e => e.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}