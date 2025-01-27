using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Configurations;

public class CommitteeMemberConfiguration : IEntityTypeConfiguration<CommitteeMember>
{
    public void Configure(EntityTypeBuilder<CommitteeMember> builder)
    {
        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Id).ValueGeneratedNever().IsRequired();

        builder.HasOne(cm => cm.CommitteeRole)
            .WithMany(cr => cr.CommitteeMembers)
            .HasForeignKey(cm => cm.CommitteeRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(cm => cm.EventId)
            .IsRequired();

        builder.Property(cm => cm.JoinedAt).HasDefaultValueSql("NOW()");

        builder.HasOne(cm => cm.User)
            .WithMany()
            .HasForeignKey(cm => cm.UserId);

        builder.HasOne(cm => cm.Event)
            .WithMany(e => e.CommitteeMembers)
            .HasForeignKey(cm => cm.EventId);

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}