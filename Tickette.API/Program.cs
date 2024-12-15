using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text.Json;
using Tickette.Infrastructure;


namespace Tickette.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                });

            builder.Services.AddFluentValidationAutoValidation(); // Register auto-validation
            builder.Services.AddFluentValidationClientsideAdapters(); // Optional: Client-side validation support

            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            builder.AddInfrastructureServices();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseExceptionHandler("/errors");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
