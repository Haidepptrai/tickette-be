using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;
using Tickette.Domain.ValueObjects;

namespace Tickette.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasOne(ci => ci.Ticket)
            .WithMany()
            .HasForeignKey(ci => ci.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(ci => ci.Quantity)
            .IsRequired();

        builder.Property(ci => ci.Price)
            .HasConversion(
                // Convert Price to decimal when storing in the database
                v => v.Amount,
                // Convert decimal to Price when reading from the database
                v => new Price(v, "USD")) // Since we don't have currency in the database, we'll use USD
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}
