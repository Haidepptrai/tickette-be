using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Configurations;

public class TicketOrderItemConfiguration : IEntityTypeConfiguration<TicketOrderItem>
{
    public void Configure(EntityTypeBuilder<TicketOrderItem> builder)
    {
        builder.Property(e => e.Quantity)
            .IsRequired();

        builder.Property(e => e.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne<TicketOrder>()
            .WithMany(order => order.Items)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}