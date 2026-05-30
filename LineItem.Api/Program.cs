using System;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using LineItem.Repositories;
using LineItem.Repositories.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LineItem.Api;

public class Program
{
    public static void Main(string[] args)
    {
        // bootstrap logger for capturing issues during application startup
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure the services collection
            ConfigureServices(builder.Services, builder.Configuration);

            // Configure and run the application
            var application = builder.Build();
            ConfigureApplication(application);

            // Start the application
            application.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application encountered a fatal unhandled exception");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // configure logging (two-stage initialization: https://github.com/serilog/serilog-aspnetcore?tab=readme-ov-file#two-stage-initialization
        services.AddSerilog(
            (serviceProvider, logConfiguration) =>
                logConfiguration
                    .ReadFrom.Configuration(configuration)
                    .ReadFrom.Services(serviceProvider)
        );

        // configure versioning
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version")
                );
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        // Add services to the container.
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // configure cpq database
        string? cpqConnectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrEmpty(cpqConnectionString))
        {
            throw new Exception("Connection string is null or undefined!");
        }

        services.AddScoped<IExampleRepository, ExampleRepository>();

        // Configure health checks
        //TODO: need to learn more about what this does
        services.AddHealthChecks();
    }

    public static void ConfigureApplication(WebApplication application)
    {
        // Configure the HTTP request pipeline.
        if (application.Environment.IsDevelopment())
        {
            application.UseDeveloperExceptionPage();
        }

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        application.UseSwagger();
        application.UseSwaggerUI();

        // configure exception handler
        application.UseExceptionHandler("/error");

        // configure middleware
        application.UseHttpsRedirection();
        application.UseAuthorization();

        // configure controllers
        application.MapControllers();

        // configure health checks
        application.MapHealthChecks("/health");
    }
}
