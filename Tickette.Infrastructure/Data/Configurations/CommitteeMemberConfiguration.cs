using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Configurations;

public class CommitteeMemberConfiguration : IEntityTypeConfiguration<CommitteeMember>
{
    public void Configure(EntityTypeBuilder<CommitteeMember> builder)
    {
        builder.HasKey(cm => new { cm.UserId, cm.EventId });

        builder.HasOne(cm => cm.User)
            .WithMany()
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.CommitteeRole)
            .WithMany(u => u.CommitteeMembers)
            .HasForeignKey(cm => cm.CommitteeRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.Event)
            .WithMany(e => e.CommitteeMembers)
            .HasForeignKey(cm => cm.EventId);

        builder.HasQueryFilter(e => e.DeletedAt == null);

        builder.HasIndex(cm => cm.UserId);
        builder.HasIndex(cm => cm.EventId);
        builder.HasIndex(cm => cm.CommitteeRoleId);
    }
}