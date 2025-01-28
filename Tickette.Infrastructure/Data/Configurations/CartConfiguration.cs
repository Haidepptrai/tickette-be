using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasOne(c => c.User)
            .WithOne(u => u.Cart)
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasComment("0: Active, 1: Expired,2: Completed");

        builder.Property(c => c.ExpiryTime)
            .IsRequired();
    }
}