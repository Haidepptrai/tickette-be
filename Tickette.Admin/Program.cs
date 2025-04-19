using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using Tickette.Admin.Helpers;
using Tickette.Infrastructure;

namespace Tickette.Admin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers(controllerOptions =>
            {
                controllerOptions.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
            })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                });

            builder.AddInfrastructure();
            builder.AddRabbitMQSettings();
            builder.AddRedisSettings();
            builder.AddInMemoryCacheService();
            builder.AddStripeSettings();
            builder.AddS3Service();
            builder.AddMachineLearningModel();
            builder.AddEmailService();
            builder.AddSignalRService();
            builder.AddCorsService();
            builder.AddScanQrService();

            //Add Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Tickette API", Version = "v1" });

                // Add JWT Bearer Authentication
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste your valid JWT token below."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        []
                    }
                });

                // Enable Swagger annotations
                options.EnableAnnotations();
            });


            var app = builder.Build();

            app.UseAuthentication();

            app.UseAuthorization();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowDevelopment");

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}
