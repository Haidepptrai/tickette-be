using Microsoft.EntityFrameworkCore;
using Tickette.Domain.Entities;

namespace Tickette.Infrastructure.Data;

public static class Seed
{
    public static void ExecuteSeed(ModelBuilder builder)
    {
        builder.Entity<Category>().HasData(
            new Category("Live Music") { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), CreatedAt = DateTime.Now },
            new Category("Stage & Arts") { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), CreatedAt = DateTime.Now },
            new Category("Sports") { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), CreatedAt = DateTime.Now },
            new Category("Others") { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), CreatedAt = DateTime.Now }
        );

    }
}