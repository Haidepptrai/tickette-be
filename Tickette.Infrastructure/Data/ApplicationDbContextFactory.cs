using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Tickette.Domain.Common;

namespace Tickette.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var connectionString = Constant.LOCAL_CONNECTION_STRING;

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString)
                          .UseSnakeCaseNamingConvention();

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
