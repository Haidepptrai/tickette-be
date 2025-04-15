using Microsoft.EntityFrameworkCore;
using Tickette.Infrastructure.Prediction.Models;

namespace Tickette.Infrastructure.Data;

public class TrainingDbContext : DbContext
{
    public DbSet<UserEventInteraction> UserCategoryInteractions { get; set; }
    public DbSet<ModelLogs> ModelLogs { get; set; }

    public TrainingDbContext(DbContextOptions<TrainingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEventInteraction>(entity =>
        {
            entity.ToTable("user_event_interactions");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.EventId).IsRequired();
            entity.Property(e => e.EventType).IsRequired();
            entity.Property(e => e.Location).IsRequired();
            entity.Property(e => e.EventDateTime).IsRequired();
            entity.Property(e => e.Label).IsRequired();
        });

        modelBuilder.Entity<ModelLogs>(entity =>
        {
            entity.ToTable("model_logs");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();
        });
    }
}