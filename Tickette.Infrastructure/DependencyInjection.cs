using Amazon;
using Amazon.S3;
using Audit.EntityFramework;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System.Configuration;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Tickette.Application.Common.CQRS;
using Tickette.Application.Common.Interfaces;
using Tickette.Application.Common.Interfaces.Email;
using Tickette.Application.Common.Interfaces.Messaging;
using Tickette.Application.Common.Interfaces.Prediction;
using Tickette.Application.Common.Interfaces.Redis;
using Tickette.Application.Common.Interfaces.Stripe;
using Tickette.Application.Common.Models;
using Tickette.Domain.Entities;
using Tickette.Infrastructure.Authentication;
using Tickette.Infrastructure.Authorization.Handlers;
using Tickette.Infrastructure.Authorization.Requirements;
using Tickette.Infrastructure.CQRS;
using Tickette.Infrastructure.Data;
using Tickette.Infrastructure.Email;
using Tickette.Infrastructure.FileStorage;
using Tickette.Infrastructure.Hubs;
using Tickette.Infrastructure.Identity;
using Tickette.Infrastructure.Messaging;
using Tickette.Infrastructure.Messaging.Feature;
using Tickette.Infrastructure.Persistence;
using Tickette.Infrastructure.Persistence.Redis;
using Tickette.Infrastructure.Prediction;
using Tickette.Infrastructure.Services;
using static Tickette.Domain.Common.Constant;

namespace Tickette.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<AuditSaveChangesInterceptor>();

        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<ApplicationDbContext>((provider, options) =>
        {
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

            options.AddInterceptors(provider.GetRequiredService<AuditSaveChangesInterceptor>());
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
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    RoleClaimType = ClaimTypes.Role
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
                    },

                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/chat-support"))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };

            });

        builder.Services.AddAuthorization(options =>
        {
            // Admin and EventOwner inherit all lower role permissions
            var elevatedRoles = new[]
            {
                COMMITTEE_MEMBER_ROLES.EventOwner,
                COMMITTEE_MEMBER_ROLES.Admin
            };

            // Define policies where lower roles + Admin + EventOwner are allowed
            options.AddPolicy("CheckInAccess", policy =>
                policy.Requirements.Add(new EventRoleRequirement(
                    COMMITTEE_MEMBER_ROLES.CheckInStaff, elevatedRoles)));

            options.AddPolicy("CheckOutAccess", policy =>
                policy.Requirements.Add(new EventRoleRequirement(
                    COMMITTEE_MEMBER_ROLES.CheckOutStaff, elevatedRoles)));

            options.AddPolicy("RedeemAccess", policy =>
                policy.Requirements.Add(new EventRoleRequirement(
                    COMMITTEE_MEMBER_ROLES.RedeemStaff, elevatedRoles)));

            options.AddPolicy("ManagerAccess", policy =>
                policy.Requirements.Add(new EventRoleRequirement(
                    COMMITTEE_MEMBER_ROLES.Manager, elevatedRoles)));
        });

        builder.Services.AddHostedService<ConfirmEmailServiceConsumer>();

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

            SeedDatabase.SeedCategories(dbContext);
            SeedDatabase.SeedRoles(roleManager);
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
        builder.Services.AddHostedService<TicketCancelReservationConsumer>();
        builder.Services.AddHostedService<ReservationMinuteSyncService>();
        builder.Services.AddHostedService<TicketConfirmationReservationConsumer>();
        builder.Services.AddHostedService<ConfirmCreateOrderEmailService>();
        builder.Services.AddScoped<ReservationStateSyncService>();
        builder.Services.AddScoped<ReservationDbSyncHandler>();
    }

    public static void AddRedisSettings(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));

        // Register Redis Connection
        builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<RedisSettings>>().Value;

            var configOptions = new ConfigurationOptions
            {
                EndPoints = { settings.ConnectionString },
                Password = settings.Password,
                User = settings.User,

                // Connection settings
                ConnectTimeout = settings.ConnectTimeout,
                SyncTimeout = settings.SyncTimeout,
                ConnectRetry = settings.ConnectRetry,
                AbortOnConnectFail = settings.AbortOnConnectFail,

                // Security settings
                Ssl = settings.Ssl,
                AllowAdmin = settings.AllowAdmin,

                // Database settings
                DefaultDatabase = settings.DefaultDatabase
            };

            return ConnectionMultiplexer.Connect(configOptions);
        });

        // Add Redis distributed cache
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            var settings = builder.Configuration.GetSection("Redis").Get<RedisSettings>() ?? throw new SettingsPropertyNotFoundException("Cannot find Redis settings");

            options.Configuration = settings.ConnectionString;
            options.InstanceName = settings.InstanceName;

            // Set configuration options if needed
            var configOptions = new ConfigurationOptions
            {
                EndPoints = { settings.ConnectionString },
                Password = settings.Password,
                ConnectTimeout = settings.ConnectTimeout,
                SyncTimeout = settings.SyncTimeout,
                AbortOnConnectFail = settings.AbortOnConnectFail,
                Ssl = settings.Ssl,
                DefaultDatabase = settings.DefaultDatabase
            };

            configOptions.EndPoints.Add(settings.ConnectionString);

            options.ConfigurationOptions = configOptions;
        });

        // Register RedLock Factory
        builder.Services.AddSingleton<IDistributedLockFactory>(provider =>
        {
            var multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
            return RedLockFactory.Create(new List<RedLockMultiplexer> { new(multiplexer) });
        });

        builder.Services.AddScoped<LockManager>();

        builder.Services.AddSingleton<IRedisService, RedisService>();
        builder.Services.AddScoped<IReservationService, ReservationService>();
        builder.Services.AddSingleton<IAgentAvailabilityService, AgentAvailabilityService>();
        builder.Services.AddSingleton<IChatRoomManagementService, ChatRoomManagementService>();

        // Register the expired reservation cleanup service
        builder.Services.AddHostedService<ExpiredReservationCleanupService>();
    }

    public static void AddInMemoryCacheService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache(options =>
        {
            options.ExpirationScanFrequency = TimeSpan.FromMinutes(5); // Frequency to scan for expired items
        });

        builder.Services.AddScoped<ICacheService, CacheService>();
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

    public static void AddMachineLearningModel(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ITrainingModelService, TrainingModelService>();
        builder.Services.AddSingleton<IRecommendationService, RecommendationService>();
    }

    public static void AddEmailService(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

        builder.Services.TryAddScoped<IEmailService, EmailService>();
    }

    public static void AddSignalRService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSignalR(options =>
        {
            options.MaximumReceiveMessageSize = 102400; // 100 KB

            options.StreamBufferCapacity = 20; // 20 messages

            // Configure timeouts for better client experience and resource management
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);

            // Limit the number of concurrent hub methods per connection
            options.MaximumParallelInvocationsPerClient = 5;
        }).AddJsonProtocol(options =>
        {
            // Optimize JSON serialization
            options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PayloadSerializerOptions.WriteIndented = false;
        }).AddHubOptions<ChatSupportHub>(options =>
        {
            // Add user-specific options for the chat hub
            options.DisableImplicitFromServicesParameters = true;
        });
    }

    public static void AddCorsService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowDevelopment",
                policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithOrigins("http://localhost:3000", "http://localhost:3001")
            );
        });
    }
}