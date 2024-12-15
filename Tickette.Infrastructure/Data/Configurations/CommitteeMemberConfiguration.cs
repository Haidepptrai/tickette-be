using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Configurations;

public class CommitteeMemberConfiguration : IEntityTypeConfiguration<CommitteeMember>
{
    public void Configure(EntityTypeBuilder<CommitteeMember> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedNever().IsRequired();

        builder.Property(e => e.Role)
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