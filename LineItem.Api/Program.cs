using System;
using Asp.Versioning;
using Dapper;
using LineItem.Api.Middleware;
using LineItem.Repositories;
using LineItem.Repositories.Helpers;
using LineItem.Repositories.Interfaces;
using LineItem.Services;
using LineItem.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LineItem.Api;

public static class Program
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

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // configure logging (two-stage initialization: https://github.com/serilog/serilog-aspnetcore?tab=readme-ov-file#two-stage-initialization
        services.AddSerilog((serviceProvider, logConfiguration) =>
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
        services.AddControllers(options =>
        {
            // Stops ASP.NET Core from removing "Async" from action names
            options.SuppressAsyncSuffixInActionNames = false;
        });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // database
        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("Connection string is null or undefined!");

        // add repositories
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        services.AddDatabaseRepository<IUserRepository, UserRepository>(connectionString);

        // add services
        services.AddScoped<IUserService, UserService>();

        // Configure health checks
        services.AddHealthChecks();
    }

    private static void ConfigureApplication(WebApplication application)
    {
        // Configure the HTTP request pipeline.
        if (application.Environment.IsDevelopment())
            application.UseDeveloperExceptionPage();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        application.UseSwagger();
        application.UseSwaggerUI();

        // configure exception handler
        application.UseExceptionHandler("/error");

        // configure middleware
        application.UseHttpsRedirection();
        application.UseAuthentication();
        application.UseMiddleware<UserContextMiddleware>();
        application.UseAuthorization();

        // configure controllers
        application.MapControllers();

        // configure health checks
        application.MapHealthChecks("/health");
    }
}