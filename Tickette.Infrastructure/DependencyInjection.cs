using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Domain.Entities;
using Tickette.Infrastructure.Authentication;
using Tickette.Infrastructure.CQRS;
using Tickette.Infrastructure.Data;
using Tickette.Infrastructure.FileStorage;
using Tickette.Infrastructure.Identity;

namespace Tickette.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        });

        builder.Services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetService<ApplicationDbContext>() ?? throw new InvalidOperationException());

        // Add Identity
        builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                // Configure identity options here (e.g., password policies)
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<IIdentityServices, IdentityServices>();

        // Configure JWT Authentication
        builder.Services.AddScoped<ITokenService, TokenService>();

        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Missing JWT Key"));

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });


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