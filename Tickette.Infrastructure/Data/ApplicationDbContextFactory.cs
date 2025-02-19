using Microsoft.AspNetCore.Http;
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

            // Create a fake HttpContextAccessor for design-time purposes
            var httpContextAccessor = new HttpContextAccessor();

            return new ApplicationDbContext(optionsBuilder.Options, httpContextAccessor);
        }
    }
}
