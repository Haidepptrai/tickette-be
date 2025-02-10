using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data.Configurations;

public class EmailUnsubscriptionConfiguration : IEntityTypeConfiguration<EmailUnsubscription>
{
    public void Configure(EntityTypeBuilder<EmailUnsubscription> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.UnsubscribedAt)
            .IsRequired();
    }
}