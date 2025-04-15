using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Tickette.Domain.Common;

namespace Tickette.Infrastructure.Data;

public class TrainingDbContextFactory : IDesignTimeDbContextFactory<TrainingDbContext>
{
    public TrainingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrainingDbContext>();

        var connectionString = Constant.LOCAL_CONNECTRING_STRING_AI;

        optionsBuilder.UseNpgsql(
            connectionString,
            x => x.MigrationsAssembly(typeof(TrainingDbContext).Assembly.FullName)
        ).UseSnakeCaseNamingConvention();

        return new TrainingDbContext(optionsBuilder.Options);
    }
}