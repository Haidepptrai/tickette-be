using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;
using Tickette.Domain.Enums;

namespace Tickette.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LocationName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.District)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Ward)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.StreetAddress)
            .IsRequired()
            .HasMaxLength(350);

        builder.Property(e => e.Description)
            .IsRequired();

        builder.Property(e => e.Banner)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasComment("Approval Status: 0 = Pending, 1 = Approved, 2 = Rejected");

        builder.Property(e => e.EventSlug)
            .IsRequired();

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.EventOwnerStripeId)
            .HasComment("Stripe Customer ID of the event owner")
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(e => e.EventOwnerStripeId)
            .HasDatabaseName("IX_EventOwnerStripeId")
            .IsUnique(false);

        builder.HasQueryFilter(e => e.DeletedAt == null && e.Status == ApprovalStatus.Approved);
    }
}