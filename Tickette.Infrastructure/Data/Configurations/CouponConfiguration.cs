using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;
using Tickette.Domain.Enums;

namespace Tickette.Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.DiscountValue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.DiscountType)
            .IsRequired()
            .HasConversion(
                v => v.ToString(), // Convert enum to string for database
                v => (DiscountType)Enum.Parse(typeof(DiscountType), v))
            .HasMaxLength(20);

        builder.Property(c => c.ExpiryDate)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();


        // Ensures each coupon code (Code) is unique within a specific event (EventDateId). 
        // Prevents duplicates in the same event while allowing reuse across different events.
        // Named "IX_Coupon_EventId_Code" for clarity in the database.
        builder.HasIndex(c => new { c.EventId, c.Code })
            .IsUnique()
            .HasDatabaseName("IX_Coupon_EventId_Code");

        builder.HasOne(c => c.Event)
            .WithMany(e => e.Coupons)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}