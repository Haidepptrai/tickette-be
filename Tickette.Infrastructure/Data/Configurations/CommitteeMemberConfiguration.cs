using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;
using Tickette.Domain.ValueObjects;

namespace Tickette.Infrastructure.Data.Configurations;

public class CommitteeMemberConfiguration : IEntityTypeConfiguration<CommitteeMember>
{
    public void Configure(EntityTypeBuilder<CommitteeMember> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedNever().IsRequired();

        builder.Property(e => e.Role)
            .HasConversion(
                role => role.Name, // Convert to string for storage
                name => CommitteeRole.FromName(name) // Recreate `CommitteeRole` from the string
            )
            .IsRequired();

        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.JoinedAt).HasDefaultValueSql("NOW()");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);

        builder.HasOne(e => e.Event)
            .WithMany()
            .HasForeignKey(e => e.EventId);
    }
}