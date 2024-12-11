using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Infrastructure.CQRS;
using Tickette.Infrastructure.Data;

namespace Tickette.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

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


    }
}