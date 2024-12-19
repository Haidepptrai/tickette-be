using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Infrastructure.CQRS;
using Tickette.Infrastructure.Data;
using Tickette.Infrastructure.FileStorage;

namespace Tickette.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        });

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetService<ApplicationDbContext>() ?? throw new InvalidOperationException());

        builder.Services.TryAddScoped<IQueryDispatcher, QueryDispatcher>();
        builder.Services.TryAddScoped<ICommandDispatcher, CommandDispatcher>();

        builder.Services.Scan(scan => scan
            .FromAssembliesOf(typeof(IQueryHandler<,>))
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        builder.Services.Scan(scan => scan
            .FromAssembliesOf(typeof(ICommandHandler<,>))
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        builder.Services.TryAddScoped<IFileStorageService, S3FileStorageService>();

        //ApplyMigrations(builder);
    }

    //private static void ApplyMigrations(IHostApplicationBuilder builder)
    //{
    //    // Create a scope to access the DbContext
    //    using var scope = builder.Services.BuildServiceProvider().CreateScope();
    //    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    //    try
    //    {
    //        dbContext.Database.Migrate(); // Apply pending migrations
    //        SeedDatabase.SeedCategories(dbContext);
    //    }
    //    catch (Exception ex)
    //    {
    //        // Log or handle migration failure
    //        Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
    //        throw; // Optionally rethrow to prevent app startup
    //    }
    //}
}