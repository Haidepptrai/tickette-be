using Amazon;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Common.Interfaces.Stripe;
using Tickette.Domain.Entities;
using Tickette.Infrastructure.Authentication;
using Tickette.Infrastructure.Authorization.Handlers;
using Tickette.Infrastructure.Authorization.Requirements;
using Tickette.Infrastructure.CQRS;
using Tickette.Infrastructure.Data;
using Tickette.Infrastructure.FileStorage;
using Tickette.Infrastructure.Identity;
using Tickette.Infrastructure.Messaging;
using Tickette.Infrastructure.Messaging.Feature;
using Tickette.Infrastructure.Persistence.Redis;
using Tickette.Infrastructure.Services;
using static Tickette.Domain.Common.Constant;

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

                // Disable claim type remapping
                options.MapInboundClaims = false;

                // Hook into events for custom behavior
                options.Events = new JwtBearerEvents
                {
                    // Customize the 401 Unauthorized response
                    OnChallenge = context =>
                    {
                        // Prevent the default behavior (which includes setting WWW-Authenticate header)
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var problemDetails = new
                        {
                            status_code = 401,
                            message = "Authentication Failed",
                            detail = "Access denied. Please provide a valid Bearer token.",
                            type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
                        };

                        return context.Response.WriteAsJsonAsync(problemDetails);
                    },

                    // Handle token validation failures
                    OnAuthenticationFailed = context =>
                    {
                        // Handle token validation failures (e.g., expired tokens)
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var problemDetails = new
                        {
                            status_code = 401,
                            message = "Authentication Failed",
                            detail = context.Exception.Message,
                            type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
                        };

                        return context.Response.WriteAsJsonAsync(problemDetails);
                    },

                    // Customize the 403 Forbidden response
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var problemDetails = new
                        {
                            status_code = 403,
                            message = "Authorization Failed",
                            detail = "You do not have permission to access this resource.",
                            type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3"
                        };

                        return context.Response.WriteAsJsonAsync(problemDetails);
                    }
                };

            });

        builder.Services.AddAuthorization(options =>
        {
            // Dynamic policy for EventOwner
            options.AddPolicy(COMMITTEE_MEMBER_ROLES.EventOwner, policy =>
                policy.Requirements.Add(new EventRoleRequirement(COMMITTEE_MEMBER_ROLES.EventOwner)));

            // Dynamic policy for Admin
            options.AddPolicy(COMMITTEE_MEMBER_ROLES.Admin, policy =>
                policy.Requirements.Add(new EventRoleRequirement(COMMITTEE_MEMBER_ROLES.Admin)));

            // Dynamic policy for Manager
            options.AddPolicy(COMMITTEE_MEMBER_ROLES.Manager, policy =>
                policy.Requirements.Add(new EventRoleRequirement(COMMITTEE_MEMBER_ROLES.Manager)));

            // Dynamic policy for CheckInStaff
            options.AddPolicy(COMMITTEE_MEMBER_ROLES.CheckInStaff, policy =>
                policy.Requirements.Add(new EventRoleRequirement(COMMITTEE_MEMBER_ROLES.CheckInStaff)));

            // Dynamic policy for CheckOutStaff
            options.AddPolicy(COMMITTEE_MEMBER_ROLES.CheckOutStaff, policy =>
                policy.Requirements.Add(new EventRoleRequirement(COMMITTEE_MEMBER_ROLES.CheckOutStaff)));

            // Dynamic policy for RedeemStaff
            options.AddPolicy(COMMITTEE_MEMBER_ROLES.RedeemStaff, policy =>
                policy.Requirements.Add(new EventRoleRequirement(COMMITTEE_MEMBER_ROLES.RedeemStaff)));
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

        builder.Services.TryAddScoped<IQrCodeService, QrCodeService>();

        // Register the custom handler and HttpContextAccessor
        builder.Services.AddScoped<IAuthorizationHandler, EventRoleHandler>();
        builder.Services.AddHttpContextAccessor();

        // Apply migrations during app initialization
        using (var scope = builder.Services.BuildServiceProvider().CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            dbContext.Database.Migrate();

            SeedDatabase.SeedCategories(dbContext).Wait();
            SeedDatabase.SeedRolesAsync(roleManager).Wait();
            SeedDatabase.SeedRolesAndPermissions(dbContext).Wait();
        }
    }

    public static void AddRabbitMQSettings(this IHostApplicationBuilder builder)
    {
        var rabbitMQSettings = new RabbitMQSettings();
        builder.Configuration.GetSection("RabbitMQ").Bind(rabbitMQSettings);

        builder.Services.AddSingleton(rabbitMQSettings);
        builder.Services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();
        builder.Services.AddSingleton<IMessageProducer, RabbitMQProducer>();
        builder.Services.AddSingleton<IMessageConsumer, RabbitMQConsumer>();

        builder.Services.AddHostedService<TicketReservationConsumer>();
    }

    public static void AddRedisSettings(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IRedisService, RedisService>();
    }

    public static void AddStripeSettings(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IPaymentService, StripePaymentService>();
    }

    public static void AddS3Service(this IHostApplicationBuilder builder)
    {
        var awsConfig = builder.Configuration.GetSection("AWS");

        // Manually create AmazonS3Config
        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(awsConfig["Region"]) // Set the region
        };

        // Manually create AmazonS3Client with AccessKey and SecretKey
        var s3Client = new AmazonS3Client(
            awsConfig["AccessKey"], // Access Key ID
            awsConfig["SecretKey"], // Secret Access Key
            s3Config
        );

        builder.Services.AddSingleton<IAmazonS3>(s3Client);

        builder.Services.TryAddScoped<IFileUploadService, S3FileUploadService>();
    }
}