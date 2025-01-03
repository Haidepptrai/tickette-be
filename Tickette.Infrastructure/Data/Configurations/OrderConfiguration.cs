using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Configurations;

public class TicketOrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BuyerEmail)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.BuyerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.BuyerPhone)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(e => e.TotalQuantity)
            .IsRequired();

        builder.Property(e => e.TotalPrice)
            .HasPrecision(18, 2);

        // Indexes
        builder.HasIndex(e => e.EventId)
            .HasDatabaseName("IX_TicketOrder_EventId");

        builder.HasIndex(e => e.BuyerEmail)
            .HasDatabaseName("IX_TicketOrder_BuyerEmail");
    }
}